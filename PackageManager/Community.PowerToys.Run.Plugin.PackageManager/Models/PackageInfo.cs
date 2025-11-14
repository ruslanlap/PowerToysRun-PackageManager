namespace Community.PowerToys.Run.Plugin.PackageManager.Models
{
    /// <summary>
    /// Unified package information across all registries
    /// </summary>
    public class PackageInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long Downloads { get; set; }
        public PackageRegistry Registry { get; set; }
        public string InstallCommand { get; set; } = string.Empty;
        public double RelevanceScore { get; set; }

        public string GetRegistryName()
        {
            return Registry switch
            {
                PackageRegistry.NPM => "NPM",
                PackageRegistry.NuGet => "NuGet",
                PackageRegistry.PyPI => "PyPI",
                _ => "Unknown"
            };
        }

        public string GetRegistryIcon()
        {
            return Registry switch
            {
                PackageRegistry.NPM => "Images\\npm.png",
                PackageRegistry.NuGet => "Images\\nuget.png",
                PackageRegistry.PyPI => "Images\\pypi.png",
                _ => "Images\\packagemanager.light.png"
            };
        }
    }
}
