using System;
using Castle.Core.Resource;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;

namespace TruffleCache.Example
{
    class Program
    {
        private static IWindsorContainer container;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting TruffleCache.Example");

            CreateContainer();

            Console.WriteLine("Created Windsor Container");

            var cache = container.Resolve<Cache<POCOObject>>();

            Console.WriteLine("Resolved Cache");

            var key = Guid.NewGuid().ToString();

            Console.WriteLine("Going to use Key " + key);

            var item = cache.Get(key);

            Console.WriteLine("Item should be null, and is " + item);

            cache.Set(key, new POCOObject { Name = "Hey TruffleMuffin" });

            item = cache.Get(key);

            Console.WriteLine("Item should be something, and is " + item);

            cache.Remove(key);
            item = cache.Get(key);

            Console.WriteLine("Item should be null, and is " + item);

            Console.WriteLine("Done.");
        }

        static void CreateContainer()
        {
            container = new WindsorContainer(new XmlInterpreter(new ConfigResource("castle")));
        }
    }

    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            //container.Register(
            //    Component
            //        .For<ICacheStore>()
            //        .ImplementedBy<MemcachedStore>()
            //        .DependsOn(
            //            Dependency.OnValue("client",
            //                new MemcachedClient(
            //                    "localhost:11211",
            //                    new MemcachedOptions
            //                    {
            //                        ConnectTimeout = TimeSpan.FromSeconds(2),
            //                        ReceiveTimeout = TimeSpan.FromSeconds(2),
            //                        EnablePipelining = true,
            //                        MaxConnections = 2,
            //                        MaxConcurrentRequestPerConnection = 15
            //                    }
            //                )
            //            )
            //        )
            //);
            container.Register(Component.For<Cache<POCOObject>>().DependsOn(Dependency.OnValue("cachePrefix", "Example.POCOObjects")));
        }
    }
}
