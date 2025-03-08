using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace SortPhotos.Core
{
    public static class Extensions
    {
        public static (Seq<T> Matched, Seq<T> Unmatched) Separate<T>(
            this IEnumerable<T> source, Func<T, bool> predicate)
        {
            // For better performance with multiple enumerations
            var sourceArray = source as T[] ?? source.ToArray();

            return (
                Matched: toSeq(sourceArray.Where(predicate)),
                Unmatched: toSeq(sourceArray.Where(item => !predicate(item)))
            );
        }

        public static bool HasValue(this string? value) =>
            !string.IsNullOrWhiteSpace(value);

        public static IO<A> Safe<A>(this IO<A> ma) =>
            ma
            | @catch(e => IO.fail<A>(AppErrors.ThereWasAProblem(e)));
    }

}
