namespace EasyContainer.Lib.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class EnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var e in enumerable) action(e);
        }
    }
}