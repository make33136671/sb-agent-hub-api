# AI Cache System

## Config (`appsettings.json`)

```json
{ "CacheConfig": { "Enabled": false, "Provider": "Memory|Redis", "Redis": { "Connection": "localhost:6379", "InstanceName": "SB:" } } }
```

## Implementations

| `Enabled` | `Provider` | Injected Service | Behavior |
|-----------|-----------|------------------|----------|
| false | — | `CacheNullService` | All ops are no-op, Get returns null/default |
| true | Memory | `CacheMemoryCacheService` | In-memory cache |
| true | Redis | `CacheRedisService` | Redis-backed cache |

## ICacheService Interface

```csharp
T Get<T>(string key);
void Set<T>(string key, T value, TimeSpan? expiration = null);
void Remove(string key);
void RemoveByPrefix(string keyPrefix);
```

## CacheNullService (Null Object Pattern)

All methods are safe no-ops. No exceptions thrown regardless of input (null key, null value, etc.). `Get<T>` always returns `default` (null for reference types).

Every request hits database when cache is disabled — expected behavior for dev/test.

## Usage Pattern

```csharp
var cached = _cacheService.Get<T>(cacheKey);  // null when disabled
if (cached == null)
{
    cached = LoadFromDatabase();
    _cacheService.Set(cacheKey, cached, expiration);  // no-op when disabled
}
return cached;
```

## Related Files

- `SB.Core/Cache/ICacheService.cs` — Interface
- `SB.Core/Cache/CacheNullService.cs` — No-op implementation
- `SB.Core/Cache/CacheMemoryCacheService.cs` — Memory implementation
- `SB.Core/Cache/CacheRedisService.cs` — Redis implementation
