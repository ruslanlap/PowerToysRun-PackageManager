using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Community.PowerToys.Run.Plugin.PackageManager.Models;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.PackageManager.Services
{
    /// <summary>
    /// NPM Registry API client
    /// API Documentation: https://github.com/npm/registry/blob/master/docs/REGISTRY-API.md
    /// </summary>
    public class NpmRegistryClient : IRegistryClient
    {
        private readonly HttpClient _httpClient;
        private const string SearchApiUrl = "https://registry.npmjs.org/-/v1/search";

        public PackageRegistry Registry => PackageRegistry.NPM;

        public NpmRegistryClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<List<PackageInfo>> SearchAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return new List<PackageInfo>();
                }

                Log.Info($"NPM: Searching for '{query}'", typeof(NpmRegistryClient));

                var allPackages = new HashSet<string>();
                var packages = new List<PackageInfo>();

                // Generate search variations to improve results
                var searchQueries = GenerateSearchQueries(query);
                
                // Limit to first few queries to avoid too many API calls
                var maxQueriesToExecute = Math.Min(8, searchQueries.Count);

                for (int i = 0; i < maxQueriesToExecute && !cancellationToken.IsCancellationRequested; i++)
                {
                    var searchQuery = searchQueries[i];
                    
                    try
                    {
                        var sizeParam = Math.Max(maxResults, 20);
                        var url = $"{SearchApiUrl}?text={Uri.EscapeDataString(searchQuery)}&size={sizeParam}";
                        var response = await _httpClient.GetFromJsonAsync<NpmSearchResponse>(url, cancellationToken);

                        if (response?.Objects != null)
                        {
                            foreach (var obj in response.Objects)
                            {
                                // Skip if we already have this package
                                if (!allPackages.Add(obj.Package.Name))
                                    continue;

                                var relevanceScore = Math.Min(1.0, obj.SearchScore * obj.Score.Final);
                                
                                // Prioritize exact name matches
                                if (string.Equals(obj.Package.Name, query, StringComparison.OrdinalIgnoreCase))
                                {
                                    relevanceScore = 1.0;
                                }
                                // Boost score if package name contains the original query
                                else if (obj.Package.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                                {
                                    relevanceScore = Math.Min(1.0, relevanceScore * 1.2);
                                }

                                packages.Add(new PackageInfo
                                {
                                    Name = obj.Package.Name,
                                    Version = obj.Package.Version,
                                    Description = obj.Package.Description ?? "No description available",
                                    Author = obj.Package.Author?.Name ?? obj.Package.Publisher?.Username ?? "Unknown",
                                    Url = obj.Package.Links.Npm ?? $"https://www.npmjs.com/package/{obj.Package.Name}",
                                    Downloads = 0,
                                    Registry = PackageRegistry.NPM,
                                    InstallCommand = $"npm install {obj.Package.Name}",
                                    RelevanceScore = relevanceScore
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug($"NPM: Failed to search for '{searchQuery}': {ex.Message}", typeof(NpmRegistryClient));
                        // Continue with next search query
                    }
                }

                // Sort by relevance score
                packages = packages.OrderByDescending(p => p.RelevanceScore).Take(maxResults).ToList();

                Log.Info($"NPM: Found {packages.Count} packages", typeof(NpmRegistryClient));
                return packages;
            }
            catch (TaskCanceledException ex)
            {
                Log.Warn($"NPM: Search timeout for '{query}'", typeof(NpmRegistryClient));
                Log.Exception("NPM timeout", ex, typeof(NpmRegistryClient));
                return new List<PackageInfo>();
            }
            catch (HttpRequestException ex)
            {
                Log.Error($"NPM: Network error while searching for '{query}'", typeof(NpmRegistryClient));
                Log.Exception("NPM network error", ex, typeof(NpmRegistryClient));
                return new List<PackageInfo>();
            }
            catch (Exception ex)
            {
                Log.Error($"NPM: Error searching for '{query}'", typeof(NpmRegistryClient));
                Log.Exception("NPM error", ex, typeof(NpmRegistryClient));
                return new List<PackageInfo>();
            }
        }

        private List<string> GenerateSearchQueries(string query)
        {
            var queries = new List<string>();
            var cleaned = query.Trim();

            // Add original query first (highest priority)
            queries.Add(cleaned);

            // Check if query is already scoped (e.g., @google/gemini)
            var isScoped = cleaned.StartsWith("@");
            var hasCli = cleaned.EndsWith("-cli", StringComparison.OrdinalIgnoreCase);
            var hasJs = cleaned.EndsWith("-js", StringComparison.OrdinalIgnoreCase);
            var hasNode = cleaned.EndsWith("-node", StringComparison.OrdinalIgnoreCase) || cleaned.StartsWith("node-", StringComparison.OrdinalIgnoreCase);

            // If query has -cli suffix, also search without it
            if (hasCli)
            {
                var baseName = cleaned.Substring(0, cleaned.Length - 4);
                queries.Add(baseName);
                
                // Add scoped versions of base name (only top priority scopes)
                if (!isScoped)
                {
                    queries.Add($"@google/{baseName}");
                    queries.Add($"@google/{cleaned}");
                    queries.Add($"@microsoft/{baseName}");
                    queries.Add($"@microsoft/{cleaned}");
                }
            }
            else if (!isScoped)
            {
                // Add most common scoped packages first (higher priority)
                queries.Add($"@google/{cleaned}");
                queries.Add($"@microsoft/{cleaned}");
                queries.Add($"@types/{cleaned}");
                
                // Add CLI variation if not already present
                if (!hasCli)
                {
                    queries.Add($"{cleaned}-cli");
                    queries.Add($"@google/{cleaned}-cli");
                    queries.Add($"@microsoft/{cleaned}-cli");
                }

                // Add common naming patterns if not already present
                if (!hasJs && !hasNode && !hasCli)
                {
                    queries.Add($"{cleaned}-js");
                    queries.Add($"node-{cleaned}");
                }
            }

            return queries;
        }
    }
}
