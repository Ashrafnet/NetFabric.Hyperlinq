using System;
using System.Runtime.CompilerServices;

namespace NetFabric.Hyperlinq
{
    public static partial class ArrayExtensions
    {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any<TSource>(this ReadOnlySpan<TSource> source)
            => source.Length is not 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any<TSource>(this in ReadOnlySpan<TSource> source, Func<TSource, bool> predicate)
            => source.Any(new FunctionWrapper<TSource, bool>(predicate));
        
        public static bool Any<TSource, TPredicate>(this ReadOnlySpan<TSource> source, TPredicate predicate = default)
            where TPredicate : struct, IFunction<TSource, bool>
        {
            for (var index = 0; index < source.Length; index++)
            {
                if (predicate.Invoke(source[index]))
                    return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any<TSource>(this in ReadOnlySpan<TSource> source, Func<TSource, int, bool> predicate)
            => source.AnyAt(new FunctionWrapper<TSource, int, bool>(predicate));

        public static bool AnyAt<TSource, TPredicate>(this ReadOnlySpan<TSource> source, TPredicate predicate = default)
            where TPredicate : struct, IFunction<TSource, int, bool>
        {
            for (var index = 0; index < source.Length; index++)
            {
                if (predicate.Invoke(source[index], index))
                    return true;
            }
            return false;
        }
    }
}

