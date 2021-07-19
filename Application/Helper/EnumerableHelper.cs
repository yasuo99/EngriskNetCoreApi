using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Helper
{
    public static class EnumerableHelper
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) where T : class
        {
            var r = new Random((int)DateTime.Now.Ticks);
            var shuffledList = source.Select(x => new { Number = r.Next(), Item = x }).OrderBy(x => x.Number).Select(x => x.Item);
            return shuffledList.ToList();
        }
    }
}