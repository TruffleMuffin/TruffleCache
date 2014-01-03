using System;
using System.Threading.Tasks;

namespace TruffleCache
{
    /// <summary>
    /// A Check and Set implementation of <see cref="CacheBase{T}"/>. This should be used when you need to ensure other applications have not updated the value stored at the key in the cache while your application has been manipulating it.
    /// </summary>
    /// <typeparam name="T">The type of item stored in cache.</typeparam>
    /// <remarks>It is highly recommended that you construct with a keyPrefix to avoid Key collisions.</remarks>
    public sealed class CasCache<T> : CacheBase<T> where T : class
    {
        private readonly ICASCacheStore store;
        private readonly string cachePrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="CasCache{T}" /> class.
        /// </summary>
        public CasCache()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CasCache{T}" /> class.
        /// </summary>
        /// <param name="cachePrefix">The key prefix.</param>
        public CasCache(string cachePrefix) : this(new MemcachedStore(), cachePrefix)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CasCache{T}" /> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="cachePrefix">The key prefix.</param>
        public CasCache(ICASCacheStore store, string cachePrefix)
            : base(store)
        {
            this.store = store;
            this.cachePrefix = cachePrefix;
        }

        /// <summary>
        /// Gets the specified item from the cache.
        /// </summary>
        /// <param name="key">The unqiue key to use for the item in the cache.</param>
        /// <returns>
        /// The instance that was retrieved from the cache and its check result
        /// </returns>
        public CheckResult<T> GetWithCheck(string key)
        {
            return Task.Run(() => GetWithCheckAsync(key)).Result;
        }

        /// <summary>
        /// Gets the specified item from the cache.
        /// </summary>
        /// <param name="key">The unqiue key to use for the item in the cache.</param>
        /// <returns>
        /// The instance that was retrieved from the cache and its check result
        /// </returns>
        public Task<CheckResult<T>> GetWithCheckAsync(string key)
        {
            return store.GetWithCheckAsync<T>(ProcessKey(key));
        }

        /// <summary>
        /// Updates an item in the cache with the default expiry.
        /// </summary>
        /// <param name="key">The unique key for the item.</param>
        /// <param name="checkValue">The check value.</param>
        /// <param name="value">The instance of the object to add to the cache.</param>
        /// <returns></returns>
        public bool Set(string key, long checkValue, T value)
        {
            return Set(key, checkValue, value, defaultExpiry);
        }

        /// <summary>
        /// Updates an item in the cache
        /// </summary>
        /// <param name="key">The unique key for the item.</param>
        /// <param name="checkValue">The check value.</param>
        /// <param name="value">The instance of the object to add to the cache.</param>
        /// <param name="expiresIn">The duration after which the item will expire from the cache.</param>
        /// <returns></returns>
        public bool Set(string key, long checkValue, T value, TimeSpan expiresIn)
        {
            return Task.Run(() => SetAsync(key, checkValue, value, expiresIn)).Result;
        }

        /// <summary>
        /// Updates an item in the cache with the default expiry
        /// </summary>
        /// <param name="key">The unique key for the item.</param>
        /// <param name="checkValue">The check value.</param>
        /// <param name="value">The instance of the object to add to the cache.</param>
        /// <returns></returns>
        public Task<bool> SetAsync(string key, long checkValue, T value)
        {
            return SetAsync(key, checkValue, value, defaultExpiry);
        }

        /// <summary>
        /// Updates an item in the cache
        /// </summary>
        /// <param name="key">The unique key for the item.</param>
        /// <param name="checkValue">The check value.</param>
        /// <param name="value">The instance of the object to add to the cache.</param>
        /// <param name="expiresIn">The duration after which the item will expire from the cache.</param>
        /// <returns></returns>
        public Task<bool> SetAsync(string key, long checkValue, T value, TimeSpan expiresIn)
        {
            return store.SetAsync(ProcessKey(key), checkValue, value, expiresIn);
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