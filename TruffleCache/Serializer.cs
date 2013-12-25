using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace TruffleCache
{
    /// <summary>
    /// Implements a Serialisation system for arbitary objects
    /// </summary>
    internal static class Serializer
    {
        private static readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        /// <summary>
        /// Asynchronously serializes the provided object.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The serialized object</returns>
        public static Task<byte[]> SerializeAsync(object value)
        {
            return Task.Run(() =>
                {
                    using (var ms = new MemoryStream())
                    {
                        binaryFormatter.Serialize(ms, value);
                        return ms.ToArray();
                    }
                });
        }

        /// <summary>
        /// Asynchronously deserializes the provided byte array.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The object</returns>
        public static Task<object> DeserializeAsync(Stream value)
        {
            return Task.Run(() => binaryFormatter.Deserialize(value));
        }

        /// <summary>
        /// Synchronously serializes the provided object.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The serialized object</returns>
        public static byte[] Serialize(object value)
        {
            return Task.Run(() => SerializeAsync(value)).Result;
        }

        /// <summary>
        /// Synchronously deserializes the provided byte array.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The object</returns>
        public static object Deserialize(Stream value)
        {
            return Task.Run(() => DeserializeAsync(value)).Result;
        }

        /// <summary>
        /// Validates the specified object is serializable.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if serializable, otherwise False.</returns>
        public static bool Validate(object value)
        {
            return value.GetType().IsSerializable;
        }
    }
}