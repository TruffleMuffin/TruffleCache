using System;
using System.Security.Cryptography;
using System.Text;

namespace TruffleCache
{
    /// <summary>
    /// A special implementation. This should be used when you have long keys that exceed the limits of the <see cref="ICacheStore"/>. If you are 
    /// using MemcacheD then this limit is approximately 250 bytes. This cache will MD5 Hash Keys before attempting to use them to ensure they are smaller
    /// than the limit.
    /// </summary>
    /// <typeparam name="T">The type of item stored in cache.</typeparam>
    /// <remarks>This cache has a performance impact due to the MD5 Hash that applies to the Key on all methods.</remarks>
    public sealed class HashCache<T> : CacheBase<T> where T : class
    {
        private readonly string cachePrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashCache{T}" /> class.
        /// </summary>
        public HashCache()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashCache{T}" /> class.
        /// </summary>
        /// <param name="cachePrefix">The key prefix.</param>
        public HashCache(string cachePrefix)
        {
            this.cachePrefix = cachePrefix;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashCache{T}" /> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="cachePrefix">The key prefix.</param>
        public HashCache(ICacheStore store, string cachePrefix)
            : base(store)
        {
            this.cachePrefix = cachePrefix;
        }

        /// <summary>
        /// Gets the function to apply to Keys in this instance.
        /// </summary>
        protected override Func<string, string> ProcessKey
        {
            get { return a => string.Concat(cachePrefix, "$", GetHashedKey(a).ToLower()); }
        }

        /// <summary>
        /// Gets the MD5 Hashed version of the specified Key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A string</returns>
        private static string GetHashedKey(string key)
        {
            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder();
            Array.ForEach(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(key)), a => sb.Append(a.ToString("X2")));
            return sb.ToString();
        }
    }
}