using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Community.PowerToys.Run.Plugin.PackageManager.Models
{
    /// <summary>
    /// PyPI API response models
    /// </summary>
    public class PyPISearchResponse
    {
        [JsonPropertyName("meta")]
        public PyPIMeta Meta { get; set; } = new();

        [JsonPropertyName("data")]
        public List<PyPIPackage> Data { get; set; } = new();
    }

    public class PyPIMeta
    {
        [JsonPropertyName("_total")]
        public int Total { get; set; }
    }

    public class PyPIPackage
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("author_email")]
        public string AuthorEmail { get; set; } = string.Empty;

        [JsonPropertyName("project_url")]
        public string ProjectUrl { get; set; } = string.Empty;

        [JsonPropertyName("package_url")]
        public string PackageUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// PyPI Package Detail API response (for single package lookup)
    /// </summary>
    public class PyPIPackageDetail
    {
        [JsonPropertyName("info")]
        public PyPIInfo Info { get; set; } = new();
    }

    public class PyPIInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("home_page")]
        public string HomePage { get; set; } = string.Empty;

        [JsonPropertyName("project_url")]
        public string ProjectUrl { get; set; } = string.Empty;

        [JsonPropertyName("package_url")]
        public string PackageUrl { get; set; } = string.Empty;
    }
}
