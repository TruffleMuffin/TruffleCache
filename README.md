TruffleCache
=====

A Cache library written to extend and make MemcachedSharp easier to use.

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
