using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Community.PowerToys.Run.Plugin.PackageManager.Models;

namespace Community.PowerToys.Run.Plugin.PackageManager.Services
{
    /// <summary>
    /// Interface for package registry clients
    /// </summary>
    public interface IRegistryClient
    {
        /// <summary>
        /// Search for packages matching the query
        /// </summary>
        Task<List<PackageInfo>> SearchAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the registry type this client handles
        /// </summary>
        PackageRegistry Registry { get; }
    }
}
