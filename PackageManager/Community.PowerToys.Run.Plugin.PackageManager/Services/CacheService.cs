using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Community.PowerToys.Run.Plugin.PackageManager.Models;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.PackageManager.Services
{
    /// <summary>
    /// In-memory caching service for package search results
    /// </summary>
    public class CacheService
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
        private readonly int _maxCacheSize = 100;

        public bool TryGetCachedResults(string query, out List<PackageInfo> results)
        {
            results = new List<PackageInfo>();

            if (string.IsNullOrWhiteSpace(query))
            {
                return false;
            }

            var cacheKey = GenerateCacheKey(query);

            if (_cache.TryGetValue(cacheKey, out var entry))
            {
                if (DateTime.UtcNow - entry.Timestamp < _cacheDuration)
                {
                    Log.Info($"Cache hit for query: '{query}'", typeof(CacheService));
                    results = entry.Results;
                    return true;
                }
                else
                {
                    // Entry expired, remove it
                    _cache.TryRemove(cacheKey, out _);
                    Log.Info($"Cache expired for query: '{query}'", typeof(CacheService));
                }
            }

            return false;
        }

        public void CacheResults(string query, List<PackageInfo> results)
        {
            if (string.IsNullOrWhiteSpace(query) || results == null)
            {
                return;
            }

            // Enforce cache size limit
            if (_cache.Count >= _maxCacheSize)
            {
                CleanupOldestEntries();
            }

            var cacheKey = GenerateCacheKey(query);
            var entry = new CacheEntry
            {
                Results = results,
                Timestamp = DateTime.UtcNow
            };

            _cache.AddOrUpdate(cacheKey, entry, (key, oldValue) => entry);
            Log.Info($"Cached {results.Count} results for query: '{query}'", typeof(CacheService));
        }

        public void ClearCache()
        {
            _cache.Clear();
            Log.Info("Cache cleared", typeof(CacheService));
        }

        private string GenerateCacheKey(string query)
        {
            return query.ToLowerInvariant().Trim();
        }

        private void CleanupOldestEntries()
        {
            try
            {
                // Remove oldest 20% of entries when cache is full
                var entriesToRemove = _cache
                    .OrderBy(kvp => kvp.Value.Timestamp)
                    .Take(_maxCacheSize / 5)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in entriesToRemove)
                {
                    _cache.TryRemove(key, out _);
                }

                Log.Info($"Cleaned up {entriesToRemove.Count} cache entries", typeof(CacheService));
            }
            catch (Exception ex)
            {
                Log.Exception("Error cleaning up cache", ex, typeof(CacheService));
            }
        }

        private class CacheEntry
        {
            public List<PackageInfo> Results { get; set; } = new();
            public DateTime Timestamp { get; set; }
        }
    }
}
