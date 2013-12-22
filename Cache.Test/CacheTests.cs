using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MbUnit.Framework;
using MemcachedSharp;

namespace Cache.Test
{
    [TestFixture]
    class CacheTests
    {
        private Cache<POCOObject> target;

        [SetUp]
        void SetUp()
        {
            target = new Cache<POCOObject>(Guid.NewGuid().ToString());
        }

        [AsyncTest]
        async Task Basic()
        {
            using (var client = new MemcachedClient("localhost:11211"))
            {
                var foo = await client.Get("foo");
                Assert.IsNull(foo);

                await client.Set("foo", Encoding.UTF8.GetBytes("Hello, World!"));

                foo = await client.Get("foo");
                Assert.IsNotNull(foo);

                await client.Delete("foo");

                foo = await client.Get("foo");
                Assert.IsNull(foo);
            }
        }

        [AsyncTest]
        async Task Scenario_Simple()
        {
            var item = new POCOObject { Id = Guid.NewGuid() };

            var cacheItem = await target.GetAsync("one");

            Assert.IsNull(cacheItem);

            await target.SetAsync("one", item);

            cacheItem = await target.GetAsync("one");

            Assert.AreEqual(item, cacheItem);
        }
    }

    [Serializable]
    public class POCOObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public AnotherPOCOObject Item { get; set; }
        public IEnumerable<AnotherPOCOObject> Items { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is POCOObject)) return false;

            return ((POCOObject)obj).Id == Id;
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Id.ToString();
        }
    }

    [Serializable]
    public class AnotherPOCOObject
    {
        public Guid Id { get; set; }
    }
}
