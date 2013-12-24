using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TruffleCache
{
    /// <summary>
    /// Describes a Cache Store
    /// </summary>
    public interface ICacheStore : IDisposable
    {
        /// <summary>
        /// Adds the specified instance to the cache
        /// </summary>
        /// <param name="key">The unqiue key to use for the item in the cache.</param>
        /// <param name="value">The value to add in the cache.</param>
        /// <param name="expiresIn">The timespan after which the item is expired from the cache.</param>
        /// <returns>
        /// True if the item was sucessfully added to the cache, false otherwise
        /// </returns>
        Task SetAsync(string key, object value, TimeSpan expiresIn);
        
        /// <summary>
        /// Gets multiple items from the cache.
        /// </summary>
        /// <param name="keys">The keys of the items to return.</param>
        /// <returns>
        /// A dictionary of the items from the cache, with the key being
        /// used as the dictionary key
        /// </returns>
        Task<IDictionary<string, object>> GetAsync(params string[] keys);
        
        /// <summary>
        /// Gets the specified item from the cache.
        /// </summary>
        /// <typeparam name="T">The type of object in the cache</typeparam>
        /// <param name="key">The unqiue key to use for the item in the cache.</param>
        /// <returns>
        /// The instance that was retrieved from the cache
        /// </returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// Removes the specified key from the cache.
        /// </summary>
        /// <param name="key">The unqiue key to use for the item in the cache.</param>
        /// <returns>
        /// True if the item was removed from the cache, false otherwise
        /// </returns>
        Task<bool> RemoveAsync(string key);
    }
}
