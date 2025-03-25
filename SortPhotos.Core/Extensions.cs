using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;
using static MediaOrganiser.Core.Types;
using LanguageExt.Traits;
using System.IO;
using static MediaOrganiser.Core.AppErrors;

namespace MediaOrganiser.Core
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
            let separatedErrors = infos.Fails.SeparateUserErrors()
            from _ in separatedErrors.Unexpected.Traverse(IO.fail<A>) // Traverse prevents early-out on any item in collection having an error
            select (infos.Succs, separatedErrors.User);     
    }
}