namespace TruffleCache
{
    /// <summary>
    /// Describes a result from cache of type {T} with its check value.
    /// </summary>
    /// <typeparam name="T">The type of object that the result is</typeparam>
    public class CheckResult<T> where T : class
    {
        /// <summary>
        /// Gets or sets the actual cache result.
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Gets or sets the check value for the cache result.
        /// </summary>
        public long CheckValue { get; set; }
    }
}