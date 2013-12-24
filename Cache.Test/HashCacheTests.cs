using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MbUnit.Framework;

namespace Cache.Test
{
    [TestFixture]
    class HashCacheTests
    {
        private HashCache<POCOObject> target;
        private const string keyPrefix = "F4F02D91-9E73-46D9-BD36-573088B52443-F4F02D91-9E73-46D9-BD36-573088B52443-F4F02D91-9E73-46D9-BD36-573088B52443-F4F02D91-9E73-46D9-BD36-573088B52443-F4F02D91-9E73-46D9-BD36-573088B52443-F4F02D91-9E73-46D9-BD36-573088B52443-F4F02D91-9E73-46D9-BD36-573088B52443-F4F02D91-9E73-46D9-BD36-573088B52443-F4F02D91-9E73-46D9-BD36-573088B52443-F4F02D91-9E73-46D9-BD36-573088B52443-F4F02D91-9E73-46D9-BD36-573088B52443-F4F02D91-9E73-46D9-BD36-573088B52443-F4F02D91-9E73-46D9-BD36-573088B52443F4F02D91-9E73-46D9-BD36-573088B52443";

        [FixtureSetUp]
        void SetUp()
        {
            target = new HashCache<POCOObject>(Guid.NewGuid().ToString());
        }

        [Test]
        void Scenario_NonAsync_Simple()
        {
            var item = new POCOObject { Id = Guid.NewGuid() };
            var key = keyPrefix + Guid.NewGuid().ToString();

            var cacheItem = target.Get(key);

            Assert.IsNull(cacheItem);

            target.Set(key, item);

            cacheItem = target.Get(key);

            Assert.AreEqual(item, cacheItem);

            var removed = target.Remove(key);
            Assert.IsTrue(removed);

            cacheItem = target.Get(key);

            Assert.IsNull(cacheItem);
        }

        [Test]
        void Scenario_Multiple_NonAsync_Simple()
        {
            var item1 = new POCOObject { Id = Guid.NewGuid() };
            var item2 = new POCOObject { Id = Guid.NewGuid() };

            var key1 = keyPrefix + Guid.NewGuid().ToString();
            var key2 = keyPrefix + Guid.NewGuid().ToString();
            var key3 = keyPrefix + Guid.NewGuid().ToString();

            var keys = new[] { key1, key2, key3 };

            var items = target.Get(keys);
            Assert.Count(3, items);
            Assert.ForAll(items, a => a.Value == null);

            target.Set(key1, item1);
            target.Set(key2, item2);

            items = target.Get(keys);
            Assert.Count(3, items);
            Assert.IsNull(items.FirstOrDefault(a => a.Key == key3).Value);
            Assert.AreEqual(item1, items.FirstOrDefault(a => a.Key == key1).Value);
            Assert.AreEqual(item2, items.FirstOrDefault(a => a.Key == key2).Value);
        }

        [AsyncTest]
        async Task Scenario_Simple()
        {
            var item = new POCOObject { Id = Guid.NewGuid() };
            var key = keyPrefix + Guid.NewGuid().ToString();

            var cacheItem = await target.GetAsync(key);

            Assert.IsNull(cacheItem);

            await target.SetAsync(key, item);

            cacheItem = await target.GetAsync(key);

            Assert.AreEqual(item, cacheItem);

            var removed = await target.RemoveAsync(key);
            Assert.IsTrue(removed);

            cacheItem = await target.GetAsync(key);

            Assert.IsNull(cacheItem);
        }

        [AsyncTest]
        async Task Scenario_DeepObject()
        {
            var item = new POCOObject
                {
                    Id = Guid.NewGuid(),
                    Item = new AnotherPOCOObject { Id = Guid.NewGuid() },
                    Items = new List<AnotherPOCOObject> { new AnotherPOCOObject { Id = Guid.NewGuid() } },
                    Name = "Hello"
                };
            var key = keyPrefix + Guid.NewGuid().ToString();

            var cacheItem = await target.GetAsync(key);

            Assert.IsNull(cacheItem);

            await target.SetAsync(key, item);

            cacheItem = await target.GetAsync(key);

            Assert.AreEqual(item, cacheItem);
            Assert.AreEqual(item.Name, cacheItem.Name);
            Assert.AreEqual(item.Item, cacheItem.Item);
            Assert.AreEqual(item.Items.First(), cacheItem.Items.First());
        }

        [AsyncTest]
        async Task Scenario_NotCached()
        {
            var cacheItem = await target.GetAsync(keyPrefix + Guid.NewGuid().ToString());
            Assert.IsNull(cacheItem);
        }

        [AsyncTest]
        async Task Scenario_Cached_TimeElapsed_NotCached()
        {
            var item = new POCOObject { Id = Guid.NewGuid() };
            var key = keyPrefix + Guid.NewGuid().ToString();
            await target.SetAsync(key, item, TimeSpan.FromSeconds(1));

            Thread.Sleep(TimeSpan.FromSeconds(2));

            var cacheItem = await target.GetAsync(key);
            Assert.IsNull(cacheItem);
        }

        [AsyncTest]
        async Task Scenario_Multiple_Simple()
        {
            var item1 = new POCOObject { Id = Guid.NewGuid() };
            var item2 = new POCOObject { Id = Guid.NewGuid() };

            var key1 = keyPrefix + Guid.NewGuid().ToString();
            var key2 = keyPrefix + Guid.NewGuid().ToString();
            var key3 = keyPrefix + Guid.NewGuid().ToString();

            var keys = new[] { key1, key2, key3 };

            var items = await target.GetAsync(keys);
            Assert.Count(3, items);
            Assert.ForAll(items, a => a.Value == null);

            await target.SetAsync(key1, item1);
            await target.SetAsync(key2, item2);

            items = await target.GetAsync(keys);
            Assert.Count(3, items);
            Assert.IsNull(items.FirstOrDefault(a => a.Key == key3).Value);
            Assert.AreEqual(item1, items.FirstOrDefault(a => a.Key == key1).Value);
            Assert.AreEqual(item2, items.FirstOrDefault(a => a.Key == key2).Value);
        }

        [AsyncTest]
        async Task Scenario_Multiple_Load()
        {
            var items = new List<POCOObject>();

            for (var i = 0; i < 5000; i++)
            {
                var item = new POCOObject { Id = Guid.NewGuid() };
                items.Add(item);
                await target.SetAsync(keyPrefix + item.Id.ToString(), item);
            }

            var keys = items.Select(a => keyPrefix + a.Id.ToString()).ToArray();

            var startAt = DateTime.Now;

            var result = await target.GetAsync(keys);

            var endAt = DateTime.Now;

            Assert.Count(items.Count, result);
            Assert.ForAll(result, a => items.FirstOrDefault(b => b.Id == a.Value.Id) != null);
            Assert.AreApproximatelyEqual(endAt - startAt, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }
    }
}