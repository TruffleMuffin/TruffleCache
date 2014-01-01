using System;
using System.Threading.Tasks;
using MbUnit.Framework;

namespace TruffleCache.Test
{
    [TestFixture]
    class CasCacheTests
    {
        private CasCache<POCOObject> target;

        [FixtureSetUp]
        void SetUp()
        {
            target = new CasCache<POCOObject>(Guid.NewGuid().ToString());
        }
        
        [Test]
        void Scenario_NonAsync_Simple()
        {
            var item = new POCOObject { Id = Guid.NewGuid() };
            var key = Guid.NewGuid().ToString();
            long checkValue = 0;

            var cacheItem = target.GetWithCheck(key);

            Assert.IsNotNull(cacheItem);
            Assert.IsNull(cacheItem.Result);
            Assert.AreEqual(0, cacheItem.CheckValue);

            target.SetAsync(key, item);

            cacheItem = target.GetWithCheck(key);

            Assert.AreEqual(item, cacheItem.Result);
            Assert.AreNotEqual(0, cacheItem.CheckValue);
            checkValue = cacheItem.CheckValue;

            var setResult = target.Set(key, checkValue, item);
            Assert.IsFalse(setResult);

            setResult = target.Set(key, checkValue, item);
            Assert.IsTrue(setResult);

            var removed = target.Remove(key);
            Assert.IsTrue(removed);

            cacheItem = target.GetWithCheck(key);

            Assert.IsNotNull(cacheItem);
            Assert.IsNull(cacheItem.Result);
            Assert.AreEqual(0, cacheItem.CheckValue);
        }
        
        [AsyncTest]
        async Task Scenario_Cas_Simple()
        {
            var item = new POCOObject { Id = Guid.NewGuid() };
            var key = Guid.NewGuid().ToString();
            long checkValue = 0;

            var cacheItem = await target.GetWithCheckAsync(key);

            Assert.IsNotNull(cacheItem);
            Assert.IsNull(cacheItem.Result);
            Assert.AreEqual(0, cacheItem.CheckValue);

            await target.SetAsync(key, item);

            cacheItem = await target.GetWithCheckAsync(key);

            Assert.AreEqual(item, cacheItem.Result);
            Assert.AreNotEqual(0, cacheItem.CheckValue);
            checkValue = cacheItem.CheckValue;

            var setResult = await target.SetAsync(key, 1, item);
            Assert.IsFalse(setResult);

            setResult = await target.SetAsync(key, checkValue, item);
            Assert.IsTrue(setResult);

            var removed = await target.RemoveAsync(key);
            Assert.IsTrue(removed);

            cacheItem = await target.GetWithCheckAsync(key);

            Assert.IsNotNull(cacheItem);
            Assert.IsNull(cacheItem.Result);
            Assert.AreEqual(0, cacheItem.CheckValue);
        }
    }
}
