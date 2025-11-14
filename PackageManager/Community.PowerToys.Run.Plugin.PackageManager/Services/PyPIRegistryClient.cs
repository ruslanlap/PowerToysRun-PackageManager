using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Community.PowerToys.Run.Plugin.PackageManager.Models;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.PackageManager.Services
{
    /// <summary>
    /// PyPI Registry API client
    /// Note: PyPI doesn't have an official search API, so we use the warehouse JSON API
    /// Alternative approach using web scraping if needed
    /// </summary>
    public class PyPIRegistryClient : IRegistryClient
    {
        private readonly HttpClient _httpClient;
        private const string SearchUrl = "https://pypi.org/search/";
        private const string PackageJsonUrl = "https://pypi.org/pypi/{0}/json";

        public PackageRegistry Registry => PackageRegistry.PyPI;

        public PyPIRegistryClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        public async Task<List<PackageInfo>> SearchAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return new List<PackageInfo>();
                }

                Log.Info($"PyPI: Searching for '{query}'", typeof(PyPIRegistryClient));

                // PyPI doesn't have a JSON search API, so we'll try to fetch the package directly if it matches
                // For a more complete implementation, you could use web scraping or the warehouse database
                var packages = new List<PackageInfo>();

                // Try direct package lookup (works for exact matches)
                try
                {
                    var packageUrl = string.Format(PackageJsonUrl, Uri.EscapeDataString(query.ToLower()));
                    var packageDetail = await _httpClient.GetFromJsonAsync<PyPIPackageDetail>(packageUrl, cancellationToken);

                    if (packageDetail?.Info != null)
                    {
                        packages.Add(new PackageInfo
                        {
                            Name = packageDetail.Info.Name,
                            Version = packageDetail.Info.Version,
                            Description = packageDetail.Info.Summary ?? "No description available",
                            Author = packageDetail.Info.Author ?? "Unknown",
                            Url = $"https://pypi.org/project/{packageDetail.Info.Name}/",
                            Downloads = 0, // PyPI JSON API doesn't provide download stats
                            Registry = PackageRegistry.PyPI,
                            InstallCommand = $"pip install {packageDetail.Info.Name}",
                            RelevanceScore = 1.0 // Exact match
                        });
                    }
                }
                catch
                {
                    // Package not found or network error - this is expected for non-exact matches
                }

                // For partial matches, we could implement web scraping of https://pypi.org/search/?q={query}
                // or use a third-party service. For now, we'll return the exact match if found.

                // Alternative: Try common variations of the query
                var variations = GeneratePackageNameVariations(query);
                foreach (var variation in variations.Take(maxResults - packages.Count))
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        var packageUrl = string.Format(PackageJsonUrl, Uri.EscapeDataString(variation));
                        var packageDetail = await _httpClient.GetFromJsonAsync<PyPIPackageDetail>(packageUrl, cancellationToken);

                        if (packageDetail?.Info != null && !packages.Any(p => p.Name.Equals(packageDetail.Info.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            packages.Add(new PackageInfo
                            {
                                Name = packageDetail.Info.Name,
                                Version = packageDetail.Info.Version,
                                Description = packageDetail.Info.Summary ?? "No description available",
                                Author = packageDetail.Info.Author ?? "Unknown",
                                Url = $"https://pypi.org/project/{packageDetail.Info.Name}/",
                                Downloads = 0,
                                Registry = PackageRegistry.PyPI,
                                InstallCommand = $"pip install {packageDetail.Info.Name}",
                                RelevanceScore = 0.8 // Variation match
                            });
                        }
                    }
                    catch
                    {
                        // Package not found - continue to next variation
                    }
                }

                Log.Info($"PyPI: Found {packages.Count} packages", typeof(PyPIRegistryClient));
                return packages;
            }
            catch (TaskCanceledException ex)
            {
                Log.Warn($"PyPI: Search timeout for '{query}'", typeof(PyPIRegistryClient));
                Log.Exception("PyPI timeout", ex, typeof(PyPIRegistryClient));
                return new List<PackageInfo>();
            }
            catch (HttpRequestException ex)
            {
                Log.Error($"PyPI: Network error while searching for '{query}'", typeof(PyPIRegistryClient));
                Log.Exception("PyPI network error", ex, typeof(PyPIRegistryClient));
                return new List<PackageInfo>();
            }
            catch (Exception ex)
            {
                Log.Error($"PyPI: Error searching for '{query}'", typeof(PyPIRegistryClient));
                Log.Exception("PyPI error", ex, typeof(PyPIRegistryClient));
                return new List<PackageInfo>();
            }
        }

        private List<string> GeneratePackageNameVariations(string query)
        {
            var variations = new List<string>();
            var cleaned = query.ToLower().Trim();

            // Add original
            variations.Add(cleaned);

            // Add common Python package naming patterns
            if (!cleaned.Contains("-"))
            {
                variations.Add(cleaned.Replace(" ", "-"));
                variations.Add(cleaned.Replace("_", "-"));
            }

            if (!cleaned.Contains("_"))
            {
                variations.Add(cleaned.Replace(" ", "_"));
                variations.Add(cleaned.Replace("-", "_"));
            }

            // Add python- prefix if not present
            if (!cleaned.StartsWith("python-"))
            {
                variations.Add($"python-{cleaned}");
            }

            // Remove duplicates
            return variations.Distinct().ToList();
        }
    }
}
