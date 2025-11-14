using System;
using System.Collections.Generic;
using System.Linq;

namespace Community.PowerToys.Run.Plugin.PackageManager.Models
{
    /// <summary>
    /// Parses user queries to extract registry filters and search terms
    /// </summary>
    public class QueryParser
    {
        private static readonly Dictionary<string, PackageRegistry> RegistryKeywords = new()
        {
            { "npm", PackageRegistry.NPM },
            { "nuget", PackageRegistry.NuGet },
            { "pip", PackageRegistry.PyPI },
            { "pypi", PackageRegistry.PyPI }
        };

        public string SearchTerm { get; set; } = string.Empty;
        public PackageRegistry? TargetRegistry { get; set; }
        public bool IsFiltered => TargetRegistry.HasValue;

        /// <summary>
        /// Parse a query string into search term and optional registry filter
        /// </summary>
        /// <param name="query">Raw query string (e.g., "npm react" or "requests")</param>
        /// <returns>Parsed query object</returns>
        public static QueryParser Parse(string query)
        {
            var result = new QueryParser();

            if (string.IsNullOrWhiteSpace(query))
            {
                return result;
            }

            var trimmed = query.Trim();
            var parts = trimmed.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return result;
            }

            // Check if first word is a registry keyword
            var firstWord = parts[0].ToLowerInvariant();
            if (RegistryKeywords.TryGetValue(firstWord, out var registry))
            {
                // Filtered search: "npm react"
                result.TargetRegistry = registry;
                result.SearchTerm = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            }
            else
            {
                // Unfiltered search: "react"
                result.SearchTerm = trimmed;
            }

            return result;
        }

        /// <summary>
        /// Get all supported registry keywords
        /// </summary>
        public static IEnumerable<string> GetSupportedKeywords()
        {
            return RegistryKeywords.Keys.OrderBy(k => k);
        }

        /// <summary>
        /// Check if a string is a valid registry keyword
        /// </summary>
        public static bool IsRegistryKeyword(string keyword)
        {
            return !string.IsNullOrWhiteSpace(keyword) &&
                   RegistryKeywords.ContainsKey(keyword.ToLowerInvariant());
        }

        /// <summary>
        /// Get registry name for display
        /// </summary>
        public string GetRegistryDisplayName()
        {
            if (!TargetRegistry.HasValue)
                return "All Registries";

            return TargetRegistry.Value switch
            {
                PackageRegistry.NPM => "NPM",
                PackageRegistry.NuGet => "NuGet",
                PackageRegistry.PyPI => "PyPI",
                _ => "Unknown"
            };
        }
    }
}
