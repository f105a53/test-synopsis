using System;
using System.Collections.Generic;
using System.Text;
using Humanizer;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;

namespace Common
{
    public static class Policies<T>
    {
        public static readonly IAsyncPolicy<T> Cache = Policy.CacheAsync<T>(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())),
            new SlidingTtl(5.Minutes()));

        public static readonly IAsyncPolicy<T> Retry = Policy<T>.Handle<TimeoutException>()
            .WaitAndRetryAsync<T>(3, _=>100.Milliseconds());

        public static readonly IAsyncPolicy<T> Complete = Policy.WrapAsync(Cache,Retry);
    }
}
