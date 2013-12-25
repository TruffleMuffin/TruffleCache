using MbUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TruffleCache.Test
{
    [TestFixture]
    class ValueCacheTests
    {
        private ValueCache<Guid> target;

        [FixtureSetUp]
        void SetUp()
        {
            target = new ValueCache<Guid>(Guid.NewGuid().ToString());
        }

        [Test]
        void Scenario_NonAsync_Simple()
        {
            var item = Guid.NewGuid();
            var key = Guid.NewGuid().ToString();

            var cacheItem = target.Get(key);

            Assert.AreEqual(default(Guid), cacheItem);

            target.Set(key, item);

            cacheItem = target.Get(key);

            Assert.AreEqual(item, cacheItem);

            var removed = target.Remove(key);
            Assert.IsTrue(removed);

            cacheItem = target.Get(key);

            Assert.AreEqual(default(Guid), cacheItem);
        }

        [Test]
        void Scenario_Multiple_NonAsync_Simple()
        {
            var item1 = Guid.NewGuid();
            var item2 = Guid.NewGuid();

            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            var key3 = Guid.NewGuid().ToString();

            var keys = new[] { key1, key2, key3 };

            var items = target.Get(keys);
            Assert.Count(3, items);
            Assert.ForAll(items, a => a.Value == default(Guid));

            target.Set(key1, item1);
            target.Set(key2, item2);

            items = target.Get(keys);
            Assert.Count(3, items);
            Assert.AreEqual(default(Guid), items.FirstOrDefault(a => a.Key == key3).Value);
            Assert.AreEqual(item1, items.FirstOrDefault(a => a.Key == key1).Value);
            Assert.AreEqual(item2, items.FirstOrDefault(a => a.Key == key2).Value);
        }

        [AsyncTest]
        async Task Scenario_Simple()
        {
            var item = Guid.NewGuid();
            var key = Guid.NewGuid().ToString();

            var cacheItem = await target.GetAsync(key);

            Assert.AreEqual(default(Guid), cacheItem);

            await target.SetAsync(key, item);

            cacheItem = await target.GetAsync(key);

            Assert.AreEqual(item, cacheItem);

            var removed = await target.RemoveAsync(key);
            Assert.IsTrue(removed);

            cacheItem = await target.GetAsync(key);

            Assert.AreEqual(default(Guid), cacheItem);
        }

        [AsyncTest]
        async Task Scenario_NotCached()
        {
            var cacheItem = await target.GetAsync(Guid.NewGuid().ToString());
            Assert.AreEqual(default(Guid), cacheItem);
        }

        [AsyncTest]
        async Task Scenario_Cached_TimeElapsed_NotCached()
        {
            var item = Guid.NewGuid();
            var key = Guid.NewGuid().ToString();
            await target.SetAsync(key, item, TimeSpan.FromSeconds(1));

            Thread.Sleep(TimeSpan.FromSeconds(2));

            var cacheItem = await target.GetAsync(key);
            Assert.AreEqual(default(Guid), cacheItem);
        }

        [AsyncTest]
        async Task Scenario_Multiple_Simple()
        {
            var item1 = Guid.NewGuid();
            var item2 = Guid.NewGuid();

            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            var key3 = Guid.NewGuid().ToString();

            var keys = new[] { key1, key2, key3 };

            var items = await target.GetAsync(keys);
            Assert.Count(3, items);
            Assert.ForAll(items, a => a.Value == default(Guid));

            await target.SetAsync(key1, item1);
            await target.SetAsync(key2, item2);

            items = await target.GetAsync(keys);
            Assert.Count(3, items);
            Assert.AreEqual(default(Guid), items.FirstOrDefault(a => a.Key == key3).Value);
            Assert.AreEqual(item1, items.FirstOrDefault(a => a.Key == key1).Value);
            Assert.AreEqual(item2, items.FirstOrDefault(a => a.Key == key2).Value);
        }

        [AsyncTest]
        async Task Scenario_Multiple_Load()
        {
            var items = new List<Guid>();

            for (var i = 0; i < 5000; i++)
            {
                var item = Guid.NewGuid();
                items.Add(item);
                await target.SetAsync(item.ToString(), item);
                Assert.AreEqual(item, await target.GetAsync(item.ToString()));
            }

            var keys = items.Select(a => a.ToString()).ToArray();

            var startAt = DateTime.Now;

            var result = await target.GetAsync(keys);

            var endAt = DateTime.Now;

            Assert.Count(items.Count, result);
            Assert.AreApproximatelyEqual(endAt - startAt, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            Assert.ForAll(result, a => a.Value != default(Guid));
            Assert.ForAll(result, a => items.FirstOrDefault(b => b == a.Value) != default(Guid));
        }
    }
}