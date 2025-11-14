using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Community.PowerToys.Run.Plugin.PackageManager.Models
{
    /// <summary>
    /// NPM Registry API response models
    /// </summary>
    public class NpmSearchResponse
    {
        [JsonPropertyName("objects")]
        public List<NpmSearchObject> Objects { get; set; } = new();

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;
    }

    public class NpmSearchObject
    {
        [JsonPropertyName("package")]
        public NpmPackage Package { get; set; } = new();

        [JsonPropertyName("score")]
        public NpmScore Score { get; set; } = new();

        [JsonPropertyName("searchScore")]
        public double SearchScore { get; set; }
    }

    public class NpmPackage
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("author")]
        public NpmAuthor? Author { get; set; }

        [JsonPropertyName("publisher")]
        public NpmPublisher? Publisher { get; set; }

        [JsonPropertyName("links")]
        public NpmLinks Links { get; set; } = new();
    }

    public class NpmAuthor
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }

    public class NpmPublisher
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }

    public class NpmLinks
    {
        [JsonPropertyName("npm")]
        public string Npm { get; set; } = string.Empty;

        [JsonPropertyName("homepage")]
        public string? Homepage { get; set; }

        [JsonPropertyName("repository")]
        public string? Repository { get; set; }
    }

    public class NpmScore
    {
        [JsonPropertyName("final")]
        public double Final { get; set; }

        [JsonPropertyName("detail")]
        public NpmScoreDetail Detail { get; set; } = new();
    }

    public class NpmScoreDetail
    {
        [JsonPropertyName("quality")]
        public double Quality { get; set; }

        [JsonPropertyName("popularity")]
        public double Popularity { get; set; }

        [JsonPropertyName("maintenance")]
        public double Maintenance { get; set; }
    }
}
