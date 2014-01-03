using System;
using System.Threading.Tasks;

namespace TruffleCache
{
    /// <summary>
    /// Describes a Cache Store that also supports Check and Set actions.
    /// </summary>
    public interface ICASCacheStore : ICacheStore
    {
        /// <summary>
        /// Adds the specified instance to the cache if the checkValue matches the one already in the cache
        /// </summary>
        /// <param name="key">The unqiue key to use for the item in the cache.</param>
        /// <param name="checkValue">The check value.</param>
        /// <param name="value">The value to add in the cache.</param>
        /// <param name="expiresIn">The timespan after which the item is expired from the cache.</param>
        /// <returns>
        /// True if the item was sucessfully added to the cache, false otherwise
        /// </returns>
        Task<bool> SetAsync(string key, long checkValue, object value, TimeSpan expiresIn);

        /// <summary>
        /// Gets the specified item from the cache with its check value.
        /// </summary>
        /// <typeparam name="T">The type of object in the cache</typeparam>
        /// <param name="key">The unqiue key to use for the item in the cache.</param>
        /// <returns>
        /// The instance that was retrieved from the cache, and its check value
        /// </returns>
        Task<CheckResult<T>> GetWithCheckAsync<T>(string key) where T : class;
    }
}