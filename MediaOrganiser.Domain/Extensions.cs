using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using System.Diagnostics;
using static LanguageExt.Prelude;
using static MediaOrganiser.Domain.AppErrors;

namespace MediaOrganiser.Domain
{
    public static class Extensions
    {
        /// <summary>
        /// Separates a sequence into two based on a predicate
        /// </summary>
        public static (Seq<T> Matched, Seq<T> Unmatched) Separate<T>(
            this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var sourceArray = source as T[] ?? source.ToArray();

            return (Matched: toSeq(sourceArray.Where(predicate)),
                    Unmatched: toSeq(sourceArray.Where(item => !predicate(item))));
        }

        /// <summary>
        /// Checks if a string has a meaningful value
        /// </summary>
        public static bool HasValue(this string? value) =>
            !string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// Wraps IO operation with safe error handling
        /// </summary>
        public static IO<A> Safe<A>(this IO<A> ma) =>
            ma | @catch(e => IO.fail<A>(ThereWasAProblem(e)));

        // Handling for expected exceptions
        public static IO<A> HandleUnauthorised<A>(this IO<A> ma, string path) =>
            ma | @catch(e => e.HasException<UnauthorizedAccessException>(), e => IO.fail<A>(UnauthorisedAccess(path, e)));

        public static IO<A> HandleEmptyPath<A>(this IO<A> ma) =>
            ma | @catch(e => e.HasException<ArgumentException>() && e.Message.Contains("path is empty"),
                e => IO.fail<A>(PathIsEmpty(e)));

        public static IO<A> HandleInvalidDirectory<A>(this IO<A> ma, string path) =>
            ma | @catch(e => e.HasException<IOException>() && e.Message.Contains("The filename, directory name, or volume label syntax is incorrect"),
                e => IO.fail<A>(DirectoryInvalid(e, path)));

        public static IO<A> HandleFileNotFound<A>(this IO<A> ma, string path) =>
            ma | @catch(e => e.HasException<FileNotFoundException>(), e => IO.fail<A>(FileNotFound(path, e)));

        public static IO<A> HandleDirectoryNotFound<A>(this IO<A> ma, string path) =>
         ma | @catch(e => e.HasException<DirectoryNotFoundException>(), e => IO.fail<A>(DirectoryNotFound(path, e)));

        /// <summary>
        /// Separates a collection of IO operations into the successes and the user errors
        /// Unexpected errors are put as fails into the monad and expected are returned as a simple error type for the user
        /// </summary>
        public static IO<(Seq<A> Succs, Seq<UserError> UserErrors)> ExtractUserErrors<A>(this Seq<IO<A>> items) =>
            from infos in items.Partition()
            let separatedErrors = SeparateUserErrors(infos.Fails)
            from _ in separatedErrors.Unexpected.Traverse(IO.fail<A>) // Traverse prevents early-out on any item in collection having an error
            select (infos.Succs, separatedErrors.User);
        public static (Seq<UserError> User, Seq<Error> Unexpected) SeparateUserErrors(this Seq<Error> allErrors)
        {
            // Log errors before converting to user safe errors
            allErrors.Iter(e =>
            {
                Debug.WriteLine(e.Message);
                e.Exception.IfSome(ex => Debug.Write(ex));
                e.Inner.IfSome(inner => Debug.Write(inner.Exception));
            });

            var separated = allErrors.Separate(err => err.Code == UserError.DisplayErrorCode);
            return (User: separated.Matched.Select(item => new UserError(item.Message)), Unexpected: separated.Unmatched);
        }
    }
}
