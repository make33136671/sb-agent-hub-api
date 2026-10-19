using Microsoft.Extensions.Caching.Memory;

namespace Sb.Apps.AgentHub.Services
{
    /// <summary>
    /// 基于 MemoryCache 的缓存实现
    /// </summary>
    public class CacheMemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly object _lockObject = new object();
        // 维护一个前缀和键的映射，用于支持 RemoveByPrefix
        private readonly Dictionary<string, HashSet<string>> _prefixKeys = new Dictionary<string, HashSet<string>>();

        public CacheMemoryCacheService(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public T? Get<T>(string key)
        {
            return _cache.TryGetValue(key, out T? value) ? value : default;
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
                options.SetAbsoluteExpiration(expiration.Value);

            _cache.Set(key, value, options);

            // 记录前缀映射关系
            lock (_lockObject)
            {
                foreach (var prefix in _prefixKeys.Keys.ToList())
                {
                    if (key.StartsWith(prefix))
                    {
                        _prefixKeys[prefix].Add(key);
                    }
                }
            }
        }

        public void Remove(string key) => _cache.Remove(key);

        public void RemoveByPrefix(string keyPrefix)
        {
            lock (_lockObject)
            {
                if (!_prefixKeys.ContainsKey(keyPrefix))
                {
                    _prefixKeys[keyPrefix] = new HashSet<string>();
                }

                var keysToRemove = _prefixKeys[keyPrefix].ToList();
                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                    _prefixKeys[keyPrefix].Remove(key);
                }
            }
        }
    }
}
