namespace EasyContainer.Lib.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public static class EnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var e in enumerable) action(e);
        }
        
        public static void ForEach(this IEnumerable enumerable, Action<object> action)
        {
            foreach (var e in enumerable) action(e);
        }
    }
}