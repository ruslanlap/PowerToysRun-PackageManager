using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Community.PowerToys.Run.Plugin.PackageManager.Models;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.PackageManager.Services
{
    /// <summary>
    /// Orchestrates package searches across multiple registries
    /// </summary>
    public class PackageSearchService : IDisposable
    {
        private readonly List<IRegistryClient> _registryClients;
        private readonly CacheService _cacheService;
        private readonly HttpClient _httpClient;
        private bool _disposed;

        public PackageSearchService()
        {
            _httpClient = new HttpClient();
            _cacheService = new CacheService();
            _registryClients = new List<IRegistryClient>
            {
                new NpmRegistryClient(_httpClient),
                new NuGetRegistryClient(_httpClient),
                new PyPIRegistryClient(_httpClient)
            };

            Log.Info("PackageSearchService initialized", typeof(PackageSearchService));
        }

        /// <summary>
        /// Search all registries in parallel
        /// </summary>
        public async Task<List<PackageInfo>> SearchAllRegistriesAsync(string query, int maxResultsPerRegistry = 5, CancellationToken cancellationToken = default, int maxTotalResults = 30)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return new List<PackageInfo>();
                }

                // Check cache first
                if (_cacheService.TryGetCachedResults(query, out var cachedResults))
                {
                    return cachedResults;
                }

                Log.Info($"Searching all registries for: '{query}'", typeof(PackageSearchService));

                // Search all registries in parallel
                var searchTasks = _registryClients.Select(client =>
                    client.SearchAsync(query, maxResultsPerRegistry, cancellationToken)
                ).ToList();

                var results = await Task.WhenAll(searchTasks);

                // Combine and rank results
                var allPackages = results
                    .SelectMany(r => r)
                    .GroupBy(p => p.Name.ToLowerInvariant())
                    .Select(g => g.OrderByDescending(p => p.RelevanceScore).ThenByDescending(p => p.Downloads).First())
                    .OrderByDescending(p => p.RelevanceScore)
                    .ThenByDescending(p => p.Downloads)
                    .ToList();

                Log.Info($"Found {allPackages.Count} total packages across all registries", typeof(PackageSearchService));

                // Cache the results
                _cacheService.CacheResults(query, allPackages);

                return allPackages;
            }
            catch (Exception ex)
            {
                Log.Exception($"Error searching for '{query}'", ex, typeof(PackageSearchService));
                return new List<PackageInfo>();
            }
        }

        /// <summary>
        /// Search a specific registry
        /// </summary>
        public async Task<List<PackageInfo>> SearchRegistryAsync(PackageRegistry registry, string query, int maxResults = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = _registryClients.FirstOrDefault(c => c.Registry == registry);
                if (client == null)
                {
                    Log.Warn($"No client found for registry: {registry}", typeof(PackageSearchService));
                    return new List<PackageInfo>();
                }

                return await client.SearchAsync(query, maxResults, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Exception($"Error searching {registry} for '{query}'", ex, typeof(PackageSearchService));
                return new List<PackageInfo>();
            }
        }

        /// <summary>
        /// Clear the cache
        /// </summary>
        public void ClearCache()
        {
            _cacheService.ClearCache();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
