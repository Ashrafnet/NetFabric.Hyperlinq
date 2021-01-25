using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetFabric.Hyperlinq
{
    public static partial class ValueEnumerable
    {
        
        public static RepeatEnumerable<TSource> Repeat<TSource>(TSource value, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

            return new RepeatEnumerable<TSource>(value, count);
        }

        [StructLayout(LayoutKind.Auto)]
        public readonly partial struct RepeatEnumerable<TSource>
            : IValueReadOnlyCollection<TSource, RepeatEnumerable<TSource>.DisposableEnumerator>
            , ICollection<TSource>
        {
            readonly TSource value;
            readonly int count;

            internal RepeatEnumerable(TSource value, int count)
            {
                this.value = value;
                this.count = count;
            }

            public readonly int Count 
                => count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly Enumerator GetEnumerator() 
                => new(in this);
            readonly DisposableEnumerator IValueEnumerable<TSource, DisposableEnumerator>.GetEnumerator() 
                => new(in this);
            readonly IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator() 
                // ReSharper disable once HeapView.BoxingAllocation
                => new DisposableEnumerator(in this);
            readonly IEnumerator IEnumerable.GetEnumerator() 
                // ReSharper disable once HeapView.BoxingAllocation
                => new DisposableEnumerator(in this);

            bool ICollection<TSource>.IsReadOnly  
                => true;

            public void CopyTo(Span<TSource> span) 
            {
                var end = count - 1;
                for (var index = 0; index <= end; index++)
                    span[index] = value;
            }

            public void CopyTo(TSource[] array, int arrayIndex)
            {
                var end = arrayIndex + count - 1;
                for (var index = arrayIndex; index <= end; index++)
                    array[index] = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(TSource item)
                => count is not 0 && EqualityComparer<TSource>.Default.Equals(value, item);

            public int IndexOf(TSource item)
                => count is not 0 && EqualityComparer<TSource>.Default.Equals(value, item)
                    ? 0
                    : -1;

            [ExcludeFromCodeCoverage]
            void ICollection<TSource>.Add(TSource item) 
                => throw new NotSupportedException();
            [ExcludeFromCodeCoverage]
            void ICollection<TSource>.Clear() 
                => throw new NotSupportedException();
            [ExcludeFromCodeCoverage]
            bool ICollection<TSource>.Remove(TSource item) 
                => throw new NotSupportedException();

            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator
            {
                int counter;
                readonly int end;

                internal Enumerator(in RepeatEnumerable<TSource> enumerable)
                {
                    Current = enumerable.value;
                    counter = -1;
                    end = counter + enumerable.Count;
                }

                public readonly TSource Current { get; }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext() 
                    => ++counter <= end;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct DisposableEnumerator
                : IEnumerator<TSource>
            {
                int counter;
                readonly int end;

                internal DisposableEnumerator(in RepeatEnumerable<TSource> enumerable)
                {
                    Current = enumerable.value;
                    counter = -1;
                    end = counter + enumerable.Count;
                }

                public readonly TSource Current { get; }
                readonly TSource IEnumerator<TSource>.Current 
                    => Current;
                readonly object? IEnumerator.Current
                    // ReSharper disable once HeapView.PossibleBoxingAllocation
                    => Current;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext() 
                    => ++counter <= end;

                [ExcludeFromCodeCoverage]
                public readonly void Reset() 
                    => throw new NotSupportedException();

                public readonly void Dispose() { }
            }
        }
    }
}

