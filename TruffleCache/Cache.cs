namespace TruffleCache
{
    /// <summary>
    /// A default implementation of the CacheBase class, which allows the key prefix to be provided on construction.
    /// </summary>
    /// <typeparam name="T">The type of item stored in cache.</typeparam>
    public class Cache<T> : CacheBase<T> where T : class
    {
        private readonly string keyPrefix;

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
        /// <param name="keyPrefix">The key prefix.</param>
        public Cache(string keyPrefix)
        {
            this.keyPrefix = keyPrefix;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache{T}" /> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        public Cache(ICacheStore store, string keyPrefix)
            : base(store)
        {
            this.keyPrefix = keyPrefix;
        }

        /// <summary>
        /// Gets the key prefix for items in this cache.
        /// </summary>
        protected override string KeyPrefix
        {
            get { return keyPrefix; }
        }
    }
}