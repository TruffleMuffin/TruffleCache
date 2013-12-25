using System;

namespace TruffleCache
{
    /// <summary>
    /// A default implementation. This should suffice for most requirements.
    /// </summary>
    /// <typeparam name="T">The type of item stored in cache.</typeparam>
    /// <remarks>It is highly recommended that you construct with a keyPrefix to avoid Key collisions.</remarks>
    public sealed class Cache<T> : CacheBase<T> where T : class
    {
        private readonly string cachePrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache{T}" /> class.
        /// </summary>
        public Cache()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache{T}" /> class.
        /// </summary>
        /// <param name="cachePrefix">The key prefix.</param>
        public Cache(string cachePrefix)
        {
            this.cachePrefix = cachePrefix;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache{T}" /> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="cachePrefix">The key prefix.</param>
        public Cache(ICacheStore store, string cachePrefix)
            : base(store)
        {
            this.cachePrefix = cachePrefix;
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