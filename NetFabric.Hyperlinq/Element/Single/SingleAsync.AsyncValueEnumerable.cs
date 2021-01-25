using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NetFabric.Hyperlinq
{
    public static partial class AsyncValueEnumerableExtensions
    {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<Option<TSource>> SingleAsync<TEnumerable, TEnumerator, TSource>(this TEnumerable source, CancellationToken cancellationToken = default) 
            where TEnumerable : IAsyncValueEnumerable<TSource, TEnumerator>
            where TEnumerator : struct, IAsyncEnumerator<TSource>
            => source.GetSingleAsync<TEnumerable, TEnumerator, TSource>(cancellationToken);

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ValueTask<Option<TSource>> SingleAsync<TEnumerable, TEnumerator, TSource, TPredicate>(this TEnumerable source, TPredicate predicate, CancellationToken cancellationToken = default) 
            where TEnumerable : IAsyncValueEnumerable<TSource, TEnumerator>
            where TEnumerator : struct, IAsyncEnumerator<TSource>
            where TPredicate : struct, IAsyncFunction<TSource, bool>
            => source.GetSingleAsync<TEnumerable, TEnumerator, TSource, TPredicate>(predicate, cancellationToken);

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ValueTask<Option<TSource>> SingleAtAsync<TEnumerable, TEnumerator, TSource, TPredicate>(this TEnumerable source, TPredicate predicate, CancellationToken cancellationToken = default) 
            where TEnumerable : IAsyncValueEnumerable<TSource, TEnumerator>
            where TEnumerator : struct, IAsyncEnumerator<TSource>
            where TPredicate : struct, IAsyncFunction<TSource, int, bool>
            => source.GetSingleAtAsync<TEnumerable, TEnumerator, TSource, TPredicate>(predicate, cancellationToken);

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static async ValueTask<Option<TResult>> SingleAsync<TEnumerable, TEnumerator, TSource, TResult, TSelector>(this TEnumerable source, TSelector selector, CancellationToken cancellationToken = default) 
            where TEnumerable : IAsyncValueEnumerable<TSource, TEnumerator>
            where TEnumerator : struct, IAsyncEnumerator<TSource>
            where TSelector : struct, IAsyncFunction<TSource, TResult>
        {
            var option = await source.GetSingleAsync<TEnumerable, TEnumerator, TSource>(cancellationToken).ConfigureAwait(false);
            return await option.SelectAsync<TResult, TSelector>(selector, cancellationToken).ConfigureAwait(false);
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static async ValueTask<Option<TResult>> SingleAtAsync<TEnumerable, TEnumerator, TSource, TResult, TSelector>(this TEnumerable source, TSelector selector, CancellationToken cancellationToken = default) 
            where TEnumerable : IAsyncValueEnumerable<TSource, TEnumerator>
            where TEnumerator : struct, IAsyncEnumerator<TSource>
            where TSelector : struct, IAsyncFunction<TSource, int, TResult>
        {
            var option = await source.GetSingleAsync<TEnumerable, TEnumerator, TSource>(cancellationToken).ConfigureAwait(false);
            return await option.SelectAsync<TResult, SingleSelector<TSource, TResult, TSelector>>(new SingleSelector<TSource, TResult, TSelector>(selector), cancellationToken).ConfigureAwait(false);
        }

        struct SingleSelector<TSource, TResult, TSelector>
            : IAsyncFunction<TSource, TResult>
            where TSelector : struct, IAsyncFunction<TSource, int, TResult>
        {
            TSelector selector;

            public SingleSelector(TSelector selector)
                => this.selector = selector;
                
            public ValueTask<TResult> InvokeAsync(TSource item, CancellationToken cancellationToken) 
                => selector.InvokeAsync(item, 0, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static async ValueTask<Option<TResult>> SingleAsync<TEnumerable, TEnumerator, TSource, TResult, TPredicate, TSelector>(this TEnumerable source, TPredicate predicate, TSelector selector, CancellationToken cancellationToken = default) 
            where TEnumerable : IAsyncValueEnumerable<TSource, TEnumerator>
            where TEnumerator : struct, IAsyncEnumerator<TSource>
            where TPredicate : struct, IAsyncFunction<TSource, bool>
            where TSelector : struct, IAsyncFunction<TSource, TResult>
        {
            var option = await source.GetSingleAsync<TEnumerable, TEnumerator, TSource, TPredicate>(predicate, cancellationToken).ConfigureAwait(false);
            return await option.SelectAsync<TResult, TSelector>(selector, cancellationToken).ConfigureAwait(false);
        }

        /////////////////////////
        // GetSingleAsync

        
        static async ValueTask<Option<TSource>> GetSingleAsync<TEnumerable, TEnumerator, TSource>(this TEnumerable source, CancellationToken cancellationToken) 
            where TEnumerable : IAsyncValueEnumerable<TSource, TEnumerator>
            where TEnumerator : struct, IAsyncEnumerator<TSource>
        {
            var enumerator = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                return await enumerator.MoveNextAsync().ConfigureAwait(false)
                    ? await enumerator.MoveNextAsync().ConfigureAwait(false) 
                        ? Option.None
                        : Option.Some(enumerator.Current)
                    : Option.None;
            }
            finally
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
        }

        
        static async ValueTask<Option<TSource>> GetSingleAsync<TEnumerable, TEnumerator, TSource, TPredicate>(this TEnumerable source, TPredicate predicate, CancellationToken cancellationToken) 
            where TEnumerable : IAsyncValueEnumerable<TSource, TEnumerator>
            where TEnumerator : struct, IAsyncEnumerator<TSource>
            where TPredicate : struct, IAsyncFunction<TSource, bool>
        {
            var enumerator = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    if (await predicate.InvokeAsync(enumerator.Current, cancellationToken).ConfigureAwait(false))
                    {
                        var value = enumerator.Current;

                        // found first, keep going until end or find second
                        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                        {
                            if (await predicate.InvokeAsync(enumerator.Current, cancellationToken).ConfigureAwait(false))
                                return Option.None;
                        }

                        return Option.Some(value);
                    }
                }

                return Option.None;
            }
            finally
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
        }

        
        static async ValueTask<Option<TSource>> GetSingleAtAsync<TEnumerable, TEnumerator, TSource, TPredicate>(this TEnumerable source, TPredicate predicate, CancellationToken cancellationToken) 
            where TEnumerable : IAsyncValueEnumerable<TSource, TEnumerator>
            where TEnumerator : struct, IAsyncEnumerator<TSource>
            where TPredicate : struct, IAsyncFunction<TSource, int, bool>
        {
            var enumerator = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                checked
                {
                    for (var index = 0; await enumerator.MoveNextAsync().ConfigureAwait(false); index++)
                    {
                        if (await predicate.InvokeAsync(enumerator.Current, index, cancellationToken).ConfigureAwait(false))
                        {
                            var value = enumerator.Current;

                            // found first, keep going until end or find second
                            for (index++; await enumerator.MoveNextAsync().ConfigureAwait(false); index++)
                            {
                                if (await predicate.InvokeAsync(enumerator.Current, index, cancellationToken).ConfigureAwait(false))
                                    return Option.None;
                            }

                            return Option.Some(value);
                        }
                    }
                }

                return Option.None;
            }
            finally
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
