using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace TruffleCache
{
    /// <summary>
    /// This is the base class for Caches implemented by TruffleCache. It provides a number of default methods and some extensibility points.
    /// If you need to implement a specialist Cache, you can extend this class to get the majority of Cache methods available from the get go.
    /// </summary>
    /// <typeparam name="T">The type of entity to store in the cache</typeparam>
    /// <remarks>Default Expiry of cache items is 24 hours. If you wish to extend this set an AppSetting for "TruffleCache.ExpiryInHours".</remarks>
    public abstract class CacheBase<T>
    {
        /// <summary>
        /// The <see cref="ICacheStore"/> this cache is using.
        /// </summary>
        protected readonly ICacheStore cache;

        /// <summary>
        /// The default expiry that this cache is using.
        /// </summary>
        protected readonly TimeSpan defaultExpiry;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheBase{T}" /> class.
        /// </summary>
        protected CacheBase()
            : this(new MemcachedStore())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheBase{T}" /> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        protected CacheBase(ICacheStore cache)
        {
            this.cache = cache;

            int customExpiry;
            if (int.TryParse(ConfigurationManager.AppSettings["TruffleCache.ExpiryInHours"], out customExpiry) == false)
            {
                customExpiry = 24;
            }
            this.defaultExpiry = TimeSpan.FromHours(customExpiry);

            if (Serializer.Validate(Activator.CreateInstance<T>()) == false)
            {
                throw new ArgumentException(string.Format("The type {0} is not Serializable", typeof(T).FullName));
            }
        }

        /// <summary>
        /// Gets the function to apply to Keys in this instance.
        /// </summary>
        protected abstract Func<string, string> ProcessKey { get; }

        /// <summary>
        /// Add/Replaces an item in the cache with the default expiry.
        /// </summary>
        /// <param name="key">The unique key for the item.</param>
        /// <param name="value">The instance of the object to add to the cache.</param>
        public virtual void Set(string key, T value)
        {
            Set(key, value, defaultExpiry);
        }

        /// <summary>
        /// Add/Replaces an item in the cache with the default expiry
        /// </summary>
        /// <param name="key">The unique key for the item.</param>
        /// <param name="value">The instance of the object to add to the cache.</param>
        public virtual Task SetAsync(string key, T value)
        {
            return SetAsync(key, value, defaultExpiry);
        }

        /// <summary>
        /// Add/Replaces an item in the cache
        /// </summary>
        /// <param name="key">The unique key for the item.</param>
        /// <param name="value">The instance of the object to add to the cache.</param>
        /// <param name="expiresIn">The duration after which the item will expire from the cache.</param>
        public virtual void Set(string key, T value, TimeSpan expiresIn)
        {
            Task.Run(() => SetAsync(key, value, expiresIn)).Wait();
        }

        /// <summary>
        /// Add/Replaces an item in the cache
        /// </summary>
        /// <param name="key">The unique key for the item.</param>
        /// <param name="value">The instance of the object to add to the cache.</param>
        /// <param name="expiresIn">The duration after which the item will expire from the cache.</param>
        public virtual Task SetAsync(string key, T value, TimeSpan expiresIn)
        {
            return cache.SetAsync(ProcessKey(key), value, expiresIn);
        }
        
        /// <summary>
        /// Removes an item with a given key from the cache
        /// </summary>
        /// <param name="key">The unique key for the item to remove.</param>
        /// <returns>True if removed, otherwise False.</returns>
        public virtual bool Remove(string key)
        {
            return Task.Run(() => RemoveAsync(key)).Result;
        }

        /// <summary>
        /// Removes an item with a given key from the cache
        /// </summary>
        /// <param name="key">The unique key for the item to remove.</param>
        /// <returns>True if removed, otherwise False.</returns>
        public virtual Task<bool> RemoveAsync(string key)
        {
            return cache.RemoveAsync(ProcessKey(key));
        }

        /// <summary>
        /// Gets an item with a given key from the cache.
        /// </summary>
        /// <param name="key">The unique key for the item.</param>
        /// <returns>The item from the cache.</returns>
        public virtual T Get(string key)
        {
            return Task.Run(() => GetAsync(key)).Result;
        }

        /// <summary>
        /// Gets an item with a given key from the cache.
        /// </summary>
        /// <param name="key">The unique key for the item.</param>
        /// <returns>The item from the cache.</returns>
        public virtual Task<T> GetAsync(string key)
        {
            return cache.GetAsync<T>(ProcessKey(key));
        }

        /// <summary>
        /// Gets multiple items from the cache.
        /// </summary>
        /// <param name="keys">The keys of the items to retrieve from cache.</param>
        /// <returns>
        /// A dictionary of the items from the cache, with the cache key being used as the dictionary key
        /// </returns>
        public virtual IDictionary<string, T> Get(params string[] keys)
        {
            return Task.Run(() => GetAsync(keys.ToArray())).Result;
        }

        /// <summary>
        /// Gets multiple items from the cache.
        /// </summary>
        /// <param name="keys">The keys of the items to retrieve from cache.</param>
        /// <returns>
        /// A dictionary of the items from the cache, with the cache key being used as the dictionary key
        /// </returns>
        public virtual async Task<IDictionary<string, T>> GetAsync(params string[] keys)
        {
            var keyLookup = keys.ToDictionary(ProcessKey, a => a);
            return (await cache.GetAsync<T>(keyLookup.Keys.ToArray())).ToDictionary(x => keyLookup[x.Key], x => x.Value);
        }

        /// <summary>
        /// Removes invalid characters from the provided key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The key with only valid characters</returns>
        protected static string CleanKey(string key)
        {
            return key.Replace(" ", "_");
        }
    }
}