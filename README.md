TruffleCache
=====

A Cache library written to extend and make [MemcachedSharp](https://github.com/bcuff/MemcachedSharp) easier to use.

Installation
-----

TruffleCache is fairly easy to install, grab the NuGet Package - TruffleCache - and add that to your project/solution. Once that is done you can use it out of the box, however this will only work against a MemcacheD server running on 127.0.0.1 on port 11211. 

To specify a custom server configuration I would recommend using an Container like Castle.Windsor. Register a custom construction of the MemcacheStore with the MemcachedClient provided. Details about the MemcachedClient configuration can be found on the [MemcachedSharp GitHub Page](https://github.com/bcuff/MemcachedSharp). An example of this method is available in the TruffleCache.Example project.

Configuration Options
-----

There are two configuration options available for TruffleCache. They are both specified by AppSettings. They are as follows

* TruffleCache.CachePrefix - Prefix all Keys used by this application with this specified key
* TruffleCache.ExpiryInHours - Default Expiry for all cache items.

Usage
-----

I have provided some example usages, one for non-async access and one for async access.

### Non-Async

```c#
private Cache<YourClass> cache;

......

var result = target.Get("myKey");
var result = target.Get(new [] { "myKey1", "myKey2" });

target.Set(key, item);

var removed = target.Remove(key);
```

### Async

```c#
private Cache<YourClass> cache;

......

var result = await target.GetAsync("myKey");
var result = await target.GetAsync(new [] { "myKey1", "myKey2" });

await target.SetAsync(key, item);

var removed = await target.RemoveAsync(key);
```

Special Caches
-----

There is currently one specialist Cache available with this library. That is the HashCache. It works the same way as the Cache class, however it will MD5 Hash all provided Keys before trying to use them with the ICacheStore implementation provided. This Cache allows you to use keys longer than 250 bytes - the Memcached limit for a key length.

Limitations
-----

There is currently a limitation with Multi Get, this is not implemented natively by [MemcachedSharp GitHub Page](https://github.com/bcuff/MemcachedSharp) so I have used a Parallel request system to imitate that functionality.

Need Something Custom?
-----

You can open an Issue here and I will try and implement it, or you can extend CacheBase<T> and implement it yourself. 

Testing
-----

A set of Unit/Integration tests are provided with this code base. If you want to test Caches implemented by this library in your own code, currently methods are marked virtual so you should be able to set up Expectation on them with most frameworks.  