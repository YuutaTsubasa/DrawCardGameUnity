using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yuuta.Core
{
    public static class LinqExtension
    {
        public static IEnumerable<(T1, T2)> ZipTuple<T1, T2>(
            this IEnumerable<T1> items1,
            IEnumerable<T2> items2)
            => items1.Zip(items2, (item1, item2) => (item1, item2));

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> items)
            => Enumerable.Range(0, items.Count())
                .Select(_ => Random.Range(int.MinValue, int.MaxValue))
                .ZipTuple(items)
                .OrderBy(itemTuple => itemTuple.Item1)
                .Select(itemTuple => itemTuple.Item2);
    }
}