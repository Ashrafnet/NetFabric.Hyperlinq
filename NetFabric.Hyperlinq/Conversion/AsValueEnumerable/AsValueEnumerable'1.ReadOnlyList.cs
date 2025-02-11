﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetFabric.Hyperlinq
{
    public static partial class ReadOnlyListExtensions
    {

        [GeneratorIgnore]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueEnumerable<IReadOnlyList<TSource>, TSource> AsValueEnumerable<TSource>(this IReadOnlyList<TSource> source)
            => new(source);

        [GeneratorIgnore]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueEnumerable<TList, TSource> AsValueEnumerable<TList, TSource>(this TList source)
            where TList : IReadOnlyList<TSource>
            => new(source);

        [StructLayout(LayoutKind.Auto)]
        public readonly partial struct ValueEnumerable<TList, TSource>
            : IValueReadOnlyList<TSource, ValueEnumerator<TSource>>
            , IList<TSource>
            where TList : IReadOnlyList<TSource>
        {
            readonly TList source;

            internal ValueEnumerable(TList source) 
                => this.source = source;

            public TSource this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => source[index];
            }

            TSource IList<TSource>.this[int index]
            {
                get => source[index];
                [DoesNotReturn]
                // ReSharper disable once ValueParameterNotUsed
                set => Throw.NotSupportedException();
            }

            public int Count
                => source.Count;

            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueEnumerator<TSource> GetEnumerator() 
                => new(source.GetEnumerator());
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator() 
                => source.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() 
                => source.GetEnumerator();

            bool ICollection<TSource>.IsReadOnly  
                => true;

            public void CopyTo(Span<TSource> span)
            {
                if (Count is 0)
                    return;
                
                if (span.Length < Count)
                    Throw.ArgumentException(Resource.DestinationNotLongEnough, nameof(span));

                using var enumerator = GetEnumerator();
                checked
                {
                    for (var index = 0; enumerator.MoveNext(); index++)
                        span[index] = enumerator.Current;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(TSource[] array, int arrayIndex)
            {
                switch (source)
                {
                    case ICollection<TSource> collection:
                        collection.CopyTo(array, arrayIndex);
                        break;
                    default:
                        CopyTo(array.AsSpan().Slice(arrayIndex));
                        break;
                }
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(TSource item)
                => Count is not 0 && source.Contains(item);

            public int IndexOf(TSource item)
            {
                return source switch
                {
                    IList<TSource> list => list.IndexOf(item),
                    _ => IndexOfEnumerable(this, item),
                };

                static int IndexOfEnumerable(ValueEnumerable<TList, TSource> source, TSource item)
                {
                    using var enumerator = source.GetEnumerator();
                    for (var index = 0; enumerator.MoveNext(); index++)
                    {
                        if (EqualityComparer<TSource>.Default.Equals(enumerator.Current, item))
                            return index;
                    }
                    return -1;
                }
            }

            [ExcludeFromCodeCoverage]
            void ICollection<TSource>.Add(TSource item) 
                => Throw.NotSupportedException();
            [ExcludeFromCodeCoverage]
            void ICollection<TSource>.Clear() 
                => Throw.NotSupportedException();
            [ExcludeFromCodeCoverage]
            bool ICollection<TSource>.Remove(TSource item) 
                => Throw.NotSupportedException<bool>();

            [ExcludeFromCodeCoverage]
            void IList<TSource>.Insert(int index, TSource item) 
                => Throw.NotSupportedException<bool>();
            [ExcludeFromCodeCoverage]
            void IList<TSource>.RemoveAt(int index) 
                => Throw.NotSupportedException<bool>();

            #region Partitioning

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SkipTakeEnumerable<ValueEnumerable<TList, TSource>, TSource> Skip(int count)
                => this.Skip<ValueEnumerable<TList, TSource>, TSource>(count);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SkipTakeEnumerable<ValueEnumerable<TList, TSource>, TSource> Take(int count)
                => this.Take<ValueEnumerable<TList, TSource>, TSource>(count);

            #endregion

            #region Conversion

            public ValueEnumerable<TList, TSource> AsValueEnumerable()
                => this;

            public IReadOnlyCollection<TSource> AsEnumerable()
                => source;

            #endregion
            
            #region Projection

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueReadOnlyListExtensions.SelectEnumerable<ValueEnumerable<TList, TSource>, ValueEnumerator<TSource>, TSource, TResult, FunctionWrapper<TSource, TResult>> Select<TResult>(Func<TSource, TResult> selector)
                => ValueReadOnlyListExtensions.Select<ValueEnumerable<TList, TSource>, ValueEnumerator<TSource>, TSource, TResult>(this, selector);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueReadOnlyListExtensions.SelectEnumerable<ValueEnumerable<TList, TSource>, ValueEnumerator<TSource>, TSource, TResult, TSelector> Select<TResult, TSelector>(TSelector selector = default)
                where TSelector : struct, IFunction<TSource, TResult>
                => ValueReadOnlyListExtensions.Select<ValueEnumerable<TList, TSource>, ValueEnumerator<TSource>, TSource, TResult, TSelector>(this, selector);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueReadOnlyListExtensions.SelectAtEnumerable<ValueEnumerable<TList, TSource>, ValueEnumerator<TSource>, TSource, TResult, FunctionWrapper<TSource, int, TResult>> Select<TResult>(Func<TSource, int, TResult> selector)
                => ValueReadOnlyListExtensions.Select<ValueEnumerable<TList, TSource>, ValueEnumerator<TSource>, TSource, TResult>(this, selector);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueReadOnlyListExtensions.SelectAtEnumerable<ValueEnumerable<TList, TSource>, ValueEnumerator<TSource>, TSource, TResult, TSelector> SelectAt<TResult, TSelector>(TSelector selector = default)
                where TSelector : struct, IFunction<TSource, int, TResult>
                => ValueReadOnlyListExtensions.SelectAt<ValueEnumerable<TList, TSource>, ValueEnumerator<TSource>, TSource, TResult, TSelector>(this, selector);

            #endregion
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<TList, TSource>(this ValueEnumerable<TList, TSource> source)
            where TList : IReadOnlyList<TSource>
            => source.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sum<TList>(this ValueEnumerable<TList, int> source)
            where TList : IReadOnlyList<int>
            => ValueReadOnlyCollectionExtensions.Sum<ValueEnumerable<TList, int>, ValueEnumerator<int>, int, int>(source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sum<TList>(this ValueEnumerable<TList, int?> source)
            where TList : IReadOnlyList<int?>
            => ValueReadOnlyCollectionExtensions.Sum<ValueEnumerable<TList, int?>, ValueEnumerator<int?>, int?, int>(source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Sum<TList>(this ValueEnumerable<TList, long> source)
            where TList : IReadOnlyList<long>
            => ValueReadOnlyCollectionExtensions.Sum<ValueEnumerable<TList, long>, ValueEnumerator<long>, long, long>(source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Sum<TList>(this ValueEnumerable<TList, long?> source)
            where TList : IReadOnlyList<long?>
            => ValueReadOnlyCollectionExtensions.Sum<ValueEnumerable<TList, long?>, ValueEnumerator<long?>, long?, long>(source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum<TList>(this ValueEnumerable<TList, float> source)
            where TList : IReadOnlyList<float>
            => ValueReadOnlyCollectionExtensions.Sum<ValueEnumerable<TList, float>, ValueEnumerator<float>, float, float>(source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sum<TList>(this ValueEnumerable<TList, float?> source)
            where TList : IReadOnlyList<float?>
            => ValueReadOnlyCollectionExtensions.Sum<ValueEnumerable<TList, float?>, ValueEnumerator<float?>, float?, float>(source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sum<TList>(this ValueEnumerable<TList, double> source)
            where TList : IReadOnlyList<double>
            => ValueReadOnlyCollectionExtensions.Sum<ValueEnumerable<TList, double>, ValueEnumerator<double>, double, double>(source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sum<TList>(this ValueEnumerable<TList, double?> source)
            where TList : IReadOnlyList<double?>
            => ValueReadOnlyCollectionExtensions.Sum<ValueEnumerable<TList, double?>, ValueEnumerator<double?>, double?, double>(source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Sum<TList>(this ValueEnumerable<TList, decimal> source)
            where TList : IReadOnlyList<decimal>
            => ValueReadOnlyCollectionExtensions.Sum<ValueEnumerable<TList, decimal>, ValueEnumerator<decimal>, decimal, decimal>(source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Sum<TList>(this ValueEnumerable<TList, decimal?> source)
            where TList : IReadOnlyList<decimal?>
            => ValueReadOnlyCollectionExtensions.Sum<ValueEnumerable<TList, decimal?>, ValueEnumerator<decimal?>, decimal?, decimal>(source);
    }
}