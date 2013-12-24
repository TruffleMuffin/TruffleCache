using System.Security.Cryptography;
using System.Text;

namespace TruffleCache
{
    /// <summary>
    /// An implementation of the Cache which will Hash Keys before adding them to the <see cref="ICacheStore" />
    /// </summary>
    /// <typeparam name="T">The type of item stored in cache.</typeparam>
    public class HashCache<T> : Cache<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HashCache{T}" /> class.
        /// </summary>
        public HashCache()
            : base(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashCache{T}" /> class.
        /// </summary>
        /// <param name="keyPrefix">The key prefix.</param>
        public HashCache(string keyPrefix)
            : base(keyPrefix)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashCache{T}" /> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        public HashCache(ICacheStore store, string keyPrefix)
            : base(store, keyPrefix)
        {
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected override string GetKey(string key)
        {
            return base.GetKey(GetCacheKey(key));
        }

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private static string GetCacheKey(string key)
        {
            // Memcache cannot have keys longer than 250 bytes in length, this includes Key Prefixes
            // Use input string to calculate MD5 hash
            // Convert the byte array to hexadecimal string
            var hashBytes = MD5Hash(key);
            var sb = new StringBuilder();
            for (var i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// MD5 Hashes the value
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private static byte[] MD5Hash(string key)
        {
            return MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(key));
        }
    }
}