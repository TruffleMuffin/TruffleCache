﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TruffleCache
{
    /// <summary>
    /// A value type implementation. This specialist cache is for use with value types.
    /// </summary>
    /// <typeparam name="T">The type of item stored in cache.</typeparam>
    /// <remarks>It is highly recommended that you construct with a keyPrefix to avoid Key collisions.</remarks>
    public sealed class ValueCache<T> : CacheBase<T> where T : struct
    {
        private readonly string cachePrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueCache{T}" /> class.
        /// </summary>
        public ValueCache()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueCache{T}" /> class.
        /// </summary>
        /// <param name="cachePrefix">The key prefix.</param>
        public ValueCache(string cachePrefix)
        {
            this.cachePrefix = cachePrefix;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueCache{T}" /> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="cachePrefix">The key prefix.</param>
        public ValueCache(ICacheStore store, string cachePrefix)
            : base(store)
        {
            this.cachePrefix = cachePrefix;
        }

        /// <summary>
        /// Gets multiple items from the cache.
        /// </summary>
        /// <param name="keys">The keys of the items to retrieve from cache.</param>
        /// <returns>
        /// A dictionary of the items from the cache, with the cache key being used as the dictionary key
        /// </returns>
        public override async Task<IDictionary<string, T>> GetAsync(params string[] keys)
        {
            var keyLookup = keys.ToDictionary(ProcessKey, a => a);
            return (await cache.GetAsync(keyLookup.Keys.ToArray())).ToDictionary(x => keyLookup[x.Key], x => x.Value == null ? default(T) : (T)x.Value);
        }

        /// <summary>
        /// Gets the function to apply to Keys in this instance.
        /// </summary>
        protected override Func<string, string> ProcessKey
        {
            get { return a => string.Concat(cachePrefix, "$", CleanKey(a).ToLower()); }
        }
    }
}