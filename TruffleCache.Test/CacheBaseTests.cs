using System;
using System.Configuration;
using System.Threading.Tasks;
using MbUnit.Framework;
using Rhino.Mocks;

namespace TruffleCache.Test
{
    [TestFixture]
    class CacheBaseTests
    {
        private MockRepository mocks;
        private Cache<POCOObject> target;
        private ICacheStore mockStore;

        [SetUp]
        void SetUp()
        {
            mocks = new MockRepository();
            mockStore = mocks.StrictMock<ICacheStore>();
        }

        [TearDown]
        void TearDown()
        {
            ConfigurationManager.AppSettings["TruffleCache.ExpiryInHours"] = null;
        }

        [Test]
        [ExpectedArgumentException]
        void Construct_NotSerializableObject_Exception()
        {
            new Cache<NotSerializableObject>();
        }

        [Test]
        void Set_DefaultExpiry_24Hours()
        {
            var item = new POCOObject();
            var key = Guid.NewGuid().ToString();
            var keyPrefix = Guid.NewGuid().ToString();

            target = new Cache<POCOObject>(mockStore, keyPrefix);

            using (mocks.Record())
            {
                Expect.Call(mockStore.SetAsync(keyPrefix + "$" + key, item, TimeSpan.FromHours(24))).Return(Task.FromResult((object)null));
            }
            using (mocks.Playback())
            {
                target.Set(key, item);
            }
        }

        [AsyncTest]
        async Task SetAsync_DefaultExpiry_24Hours()
        {
            var item = new POCOObject();
            var key = Guid.NewGuid().ToString();
            var keyPrefix = Guid.NewGuid().ToString();

            target = new Cache<POCOObject>(mockStore, keyPrefix);

            using (mocks.Record())
            {
                Expect.Call(mockStore.SetAsync(keyPrefix + "$" + key, item, TimeSpan.FromHours(24))).Return(Task.FromResult((object)null));
            }
            using (mocks.Playback())
            {
                await target.SetAsync(key, item);
            }
        }

        [Test]
        void Set_CustomExpiry_48Hours()
        {
            var item = new POCOObject();
            var key = Guid.NewGuid().ToString();
            var keyPrefix = Guid.NewGuid().ToString();

            ConfigurationManager.AppSettings["TruffleCache.ExpiryInHours"] = "48";
            
            target = new Cache<POCOObject>(mockStore, keyPrefix);

            using (mocks.Record())
            {
                Expect.Call(mockStore.SetAsync(keyPrefix + "$" + key, item, TimeSpan.FromHours(48))).Return(Task.FromResult((object)null));
            }
            using (mocks.Playback())
            {
                target.Set(key, item);
            }
        }

        [AsyncTest]
        async Task SetAsync_CustomExpiry_48Hours()
        {
            var item = new POCOObject();
            var key = Guid.NewGuid().ToString();
            var keyPrefix = Guid.NewGuid().ToString();

            ConfigurationManager.AppSettings["TruffleCache.ExpiryInHours"] = "48";

            target = new Cache<POCOObject>(mockStore, keyPrefix);

            using (mocks.Record())
            {
                Expect.Call(mockStore.SetAsync(keyPrefix + "$" + key, item, TimeSpan.FromHours(48))).Return(Task.FromResult((object)null));
            }
            using (mocks.Playback())
            {
                await target.SetAsync(key, item);
            }
        }
        
        class NotSerializableObject
        {
             
        }
    }
}