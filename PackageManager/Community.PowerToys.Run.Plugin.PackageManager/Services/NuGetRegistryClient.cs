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
    /// NuGet Registry API client
    /// API Documentation: https://docs.microsoft.com/en-us/nuget/api/search-query-service-resource
    /// </summary>
    public class NuGetRegistryClient : IRegistryClient
    {
        private readonly HttpClient _httpClient;
        private const string SearchApiUrl = "https://azuresearch-usnc.nuget.org/query";

        public PackageRegistry Registry => PackageRegistry.NuGet;

        public NuGetRegistryClient(HttpClient httpClient)
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

                var url = $"{SearchApiUrl}?q={Uri.EscapeDataString(query)}&take={maxResults}&prerelease=false";

                Log.Info($"NuGet: Searching for '{query}'", typeof(NuGetRegistryClient));

                var response = await _httpClient.GetFromJsonAsync<NuGetSearchResponse>(url, cancellationToken);

                if (response?.Data == null)
                {
                    Log.Warn("NuGet: No results returned", typeof(NuGetRegistryClient));
                    return new List<PackageInfo>();
                }

                var packages = response.Data.Select(pkg => new PackageInfo
                {
                    Name = pkg.Id,
                    Version = pkg.Version,
                    Description = pkg.Description ?? "No description available",
                    Author = pkg.Authors.FirstOrDefault() ?? "Unknown",
                    Url = pkg.ProjectUrl ?? $"https://www.nuget.org/packages/{pkg.Id}",
                    Downloads = pkg.TotalDownloads,
                    Registry = PackageRegistry.NuGet,
                    InstallCommand = $"dotnet add package {pkg.Id}",
                    // Normalize downloads to 0-1 range using logarithmic scale
                    // log10(downloads + 1) / 10 gives 0-1 for most packages (up to 10B downloads)
                    RelevanceScore = Math.Min(1.0, Math.Log10(pkg.TotalDownloads + 1) / 10.0)
                }).ToList();

                Log.Info($"NuGet: Found {packages.Count} packages", typeof(NuGetRegistryClient));
                return packages;
            }
            catch (TaskCanceledException ex)
            {
                Log.Warn($"NuGet: Search timeout for '{query}'", typeof(NuGetRegistryClient));
                Log.Exception("NuGet timeout", ex, typeof(NuGetRegistryClient));
                return new List<PackageInfo>();
            }
            catch (HttpRequestException ex)
            {
                Log.Error($"NuGet: Network error while searching for '{query}'", typeof(NuGetRegistryClient));
                Log.Exception("NuGet network error", ex, typeof(NuGetRegistryClient));
                return new List<PackageInfo>();
            }
            catch (Exception ex)
            {
                Log.Error($"NuGet: Error searching for '{query}'", typeof(NuGetRegistryClient));
                Log.Exception("NuGet error", ex, typeof(NuGetRegistryClient));
                return new List<PackageInfo>();
            }
        }
    }
}
