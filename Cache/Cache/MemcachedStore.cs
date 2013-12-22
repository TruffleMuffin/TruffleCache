using MemcachedSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Cache
{
    /// <summary>
    /// An implementation of the <see cref="ICacheStore" /> using the <see cref="MemcachedClient" />.
    /// </summary>
    public class MemcachedStore : ICacheStore
    {
        private readonly Lazy<MemcachedClient> client;
        private readonly string prefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemcachedStore" /> class.
        /// </summary>
        public MemcachedStore()
        {
            client = new Lazy<MemcachedClient>(() => new MemcachedClient("localhost:11211", new MemcachedOptions
            {
                ConnectTimeout = TimeSpan.FromSeconds(2),
                ReceiveTimeout = TimeSpan.FromSeconds(2),
                EnablePipelining = true,
                MaxConnections = 2,
                MaxConcurrentRequestPerConnection = 15
            }));
            this.prefix = ConfigurationManager.AppSettings["Application.CachePrefix"] ?? "_";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemcachedStore" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public MemcachedStore(MemcachedClient client)
        {
            this.client = new Lazy<MemcachedClient>(() => client);
            this.prefix = ConfigurationManager.AppSettings["Application.CachePrefix"] ?? "_";
        }

        /// <summary>
        /// Adds the specified instance to the cache
        /// </summary>
        /// <param name="key">The unqiue key to use for the item in the cache.</param>
        /// <param name="value">The value to add in the cache.</param>
        /// <param name="expiresIn">The timespan after which the item is expired from the cache.</param>
        /// <returns>
        /// True if the item was sucessfully added to the cache, false otherwise
        /// </returns>
        public Task SetAsync(string key, object value, TimeSpan expiresIn)
        {
            var serialize = Serializer.Serialize(value);
            return client.Value.Set(PrefixKey(key), serialize, new MemcachedStorageOptions { ExpirationTime = expiresIn });
        }

        /// <summary>
        /// Gets multiple items from the cache.
        /// </summary>
        /// <param name="keys">The keys of the items to return.</param>
        /// <returns>
        /// A dictionary of the items from the cache, with the key being
        /// used as the dictionary key
        /// </returns>
        public async Task<IDictionary<string, object>> GetAsync(params string[] keys)
        {
            var tasks = keys
                .AsParallel()
                .Select(a => client.Value.Get(PrefixKey(a)))
                .ToArray();
            var results = await Task.WhenAll(tasks);

            return results.ToDictionary(a => DePrefixKey(a.Key), a => Serializer.Deserialize(a.Data));
        }

        /// <summary>
        /// Gets the specified item from the cache.
        /// </summary>
        /// <typeparam name="T">The type of object in the cache</typeparam>
        /// <param name="key">The unqiue key to use for the item in the cache.</param>
        /// <returns>
        /// The instance that was retrieved from the cache
        /// </returns>
        public async Task<T> GetAsync<T>(string key)
        {
            var result = await client.Value.Get(PrefixKey(key));

            if (result == null) return (T)(object)null;

            return (T)await Serializer.DeserializeAsync(result.Data);
        }

        /// <summary>
        /// Removes the specified key from the cache.
        /// </summary>
        /// <param name="key">The unqiue key to use for the item in the cache.</param>
        /// <returns>
        /// True if the item was removed from the cache, false otherwise
        /// </returns>
        public Task<bool> RemoveAsync(string key)
        {
            return client.Value.Delete(PrefixKey(key));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (client.IsValueCreated)
            {
                client.Value.Dispose();
            }
        }

        /// <summary>
        /// Prefixes the key.
        /// </summary>
        /// <param name="key">The key.</param>
        private string PrefixKey(string key)
        {
            return string.Concat(prefix, key);
        }

        /// <summary>
        /// Des the prefix key.
        /// </summary>
        /// <param name="key">The key.</param>
        private string DePrefixKey(string key)
        {
            return key.Replace(prefix, string.Empty);
        }
    }
}