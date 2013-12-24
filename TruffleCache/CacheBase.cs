using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TruffleCache
{
    /// <summary>
    /// A base cache for storing a single type
    /// </summary>
    /// <typeparam name="T">The type of entity to store in the cache</typeparam>
    public abstract class CacheBase<T>
    {
        private readonly ICacheStore cache;
        private string short_key;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheBase{T}" /> class.
        /// </summary>
        protected CacheBase()
            : this(new MemcachedStore())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheBase&lt;T&gt;" /> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        protected CacheBase(ICacheStore cache)
        {
            this.cache = cache;
        }

        /// <summary>
        /// Gets the key prefix for items in this cache
        /// </summary>
        protected abstract string KeyPrefix { get; }

        /// <summary>
        /// Gets the short key prefix.
        /// </summary>
        private string ShortKey
        {
            get { return short_key ?? (short_key = string.Concat(KeyPrefix, "$").ToLower()); }
        }

        /// <summary>
        /// Add/Replaces an item in the cache
        /// </summary>
        /// <param name="key">The unique key for the item to remove</param>
        /// <param name="value">The instance of the object to add to the cache</param>
        /// <param name="expiresIn">The duration after which the item will expire from the cache.</param>
        /// <returns>
        /// True if the item was cached, false otherwise.
        /// </returns>
        public virtual void Set(string key, T value, TimeSpan expiresIn)
        {
            Task.Run(() => SetAsync(key, value, expiresIn)).Wait();
        }

        /// <summary>
        /// Add/Replaces an item in the cache
        /// </summary>
        /// <param name="key">The unique key for the item to remove</param>
        /// <param name="value">The instance of the object to add to the cache</param>
        /// <param name="expiresIn">The duration after which the item will expire from the cache.</param>
        /// <returns>
        /// True if the item was cached, false otherwise.
        /// </returns>
        public virtual Task SetAsync(string key, T value, TimeSpan expiresIn)
        {
            return cache.SetAsync(GetKey(key), value, expiresIn);
        }

        /// <summary>
        /// Add/Replaces an item in the cache
        /// </summary>
        /// <param name="key">The unique key for the item to remove</param>
        /// <param name="value">The instance of the object to add to the cache</param>
        /// <returns>
        /// True if the item was cached, false otherwise.
        /// </returns>
        public virtual void Set(string key, T value)
        {
            Set(key, value, TimeSpan.FromDays(7));
        }

        /// <summary>
        /// Add/Replaces an item in the cache
        /// </summary>
        /// <param name="key">The unique key for the item to remove</param>
        /// <param name="value">The instance of the object to add to the cache</param>
        /// <returns>
        /// True if the item was cached, false otherwise.
        /// </returns>
        public virtual Task SetAsync(string key, T value)
        {
            return SetAsync(key, value, TimeSpan.FromDays(7));
        }

        /// <summary>
        /// Removes an item with a given key from the cache
        /// </summary>
        /// <param name="key">The unique key for the item to remove</param>
        /// <returns>The item that was removed from the cache</returns>
        public virtual bool Remove(string key)
        {
            return Task.Run(() => RemoveAsync(key)).Result;
        }

        /// <summary>
        /// Removes an item with a given key from the cache
        /// </summary>
        /// <param name="key">The unique key for the item to remove</param>
        /// <returns>The item that was removed from the cache</returns>
        public virtual Task<bool> RemoveAsync(string key)
        {
            return cache.RemoveAsync(GetKey(key));
        }

        /// <summary>
        /// Gets an item with a given key from the cache.
        /// </summary>
        /// <param name="key">The unique key for the item to remove.</param>
        /// <returns>The item from the cache</returns>
        public virtual T Get(string key)
        {
            return Task.Run(() => GetAsync(key)).Result;
        }

        /// <summary>
        /// Gets an item with a given key from the cache.
        /// </summary>
        /// <param name="key">The unique key for the item to remove.</param>
        /// <returns>The item from the cache</returns>
        public virtual Task<T> GetAsync(string key)
        {
            return cache.GetAsync<T>(GetKey(key));
        }

        /// <summary>
        /// Gets multiple items from the cache.
        /// </summary>
        /// <param name="keys">The keys of the items to return.</param>
        /// <returns>
        /// A dictionary of the items from the cache, with the key being
        /// used as the dictionary key
        /// </returns>
        public virtual IDictionary<string, T> Get(params string[] keys)
        {
            return Task.Run(() => GetAsync(keys.ToArray())).Result;
        }

        /// <summary>
        /// Gets multiple items from the cache.
        /// </summary>
        /// <param name="keys">The keys of the items to return.</param>
        /// <returns>
        /// A dictionary of the items from the cache, with the key being
        /// used as the dictionary key
        /// </returns>
        public virtual async Task<IDictionary<string, T>> GetAsync(params string[] keys)
        {
            var keyLookup = keys.ToDictionary(GetKey, a => a);
            return (await cache.GetAsync(keyLookup.Keys.ToArray())).ToDictionary(x => ShortenKey(keyLookup[x.Key]), x => (T)x.Value);
        }

        /// <summary>
        /// Gets the prefixed key.
        /// </summary>
        /// <param name="key">The unique key for the item.</param>
        /// <returns>
        /// 3
        /// The key for the given entity, prefixed with
        /// the required value.
        /// </returns>
        protected virtual string GetKey(string key)
        {
            if (key == null) throw new ArgumentException("A key must be specified");

            //replace any spaces (not supported in cache keys)
            //return the key with the cache prefix, all lower case
            //this makes cache keys non-case sensitive
            return string.Concat(KeyPrefix, "$", key.Replace(" ", "__")).ToLower();
        }

        /// <summary>
        /// Gets the key with the prefix removed.
        /// </summary>
        /// <param name="key">The unique key for the item.</param>
        /// <returns>
        /// The key for the given entity, with prefix removed
        /// </returns>
        private string ShortenKey(string key)
        {
            return key.Replace(ShortKey, string.Empty);
        }
    }
}