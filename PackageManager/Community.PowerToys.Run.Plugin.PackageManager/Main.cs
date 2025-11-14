using ManagedCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Community.PowerToys.Run.Plugin.PackageManager.Models;
using Community.PowerToys.Run.Plugin.PackageManager.Services;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.PackageManager
{
    /// <summary>
    /// Main PowerToys Run plugin class for Package Manager
    /// </summary>
    public class Main : IPlugin, IContextMenu, IDisposable
    {
        /// <summary>
        /// ID of the plugin.
        /// </summary>
        public static string PluginID  => "E5B5E2D34F4C4E1A9B2F6C8A7D5E4F3B";

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "Package Manager";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "Search and install packages from NPM, NuGet, and PyPI";

        private PluginInitContext Context { get; set; } = null!;
        private string IconPath { get; set; } = null!;
        private bool Disposed { get; set; }

        private PackageSearchService? _searchService;
        private CancellationTokenSource? _searchCancellation;

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());

            _searchService = new PackageSearchService();
            Log.Info("Package Manager plugin initialized", GetType());
        }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            try
            {
                if (query == null || Context == null || _searchService == null)
                {
                    return [];
                }

                var rawQuery = query.Search?.Trim() ?? string.Empty;

                // Show help when no search term
                if (string.IsNullOrWhiteSpace(rawQuery))
                {
                    return GetDefaultResults();
                }

                // Parse query to extract registry filter and search term
                var parsedQuery = QueryParser.Parse(rawQuery);

                // If only registry keyword without search term, show help
                if (string.IsNullOrWhiteSpace(parsedQuery.SearchTerm))
                {
                    return GetFilteredHelpResults(parsedQuery);
                }

                // Cancel any ongoing search
                _searchCancellation?.Cancel();
                _searchCancellation = new CancellationTokenSource();

                // Perform search (filtered or unfiltered)
                List<PackageInfo> packages;
                if (parsedQuery.IsFiltered)
                {
                    // Filtered search: pm npm react
                    Log.Info($"Filtered search: {parsedQuery.TargetRegistry} - {parsedQuery.SearchTerm}", GetType());
                    packages = Task.Run(async () =>
                        await _searchService.SearchRegistryAsync(
                            parsedQuery.TargetRegistry!.Value,
                            parsedQuery.SearchTerm,
                            maxResults: 10,
                            _searchCancellation.Token)
                    ).GetAwaiter().GetResult();
                }
                else
                {
                    // Unfiltered search: pm react
                    Log.Info($"Unfiltered search: {parsedQuery.SearchTerm}", GetType());
                    packages = Task.Run(async () =>
                        await _searchService.SearchAllRegistriesAsync(
                            parsedQuery.SearchTerm,
                            maxResultsPerRegistry: 5,
                            _searchCancellation.Token)
                    ).GetAwaiter().GetResult();
                }

                if (!packages.Any())
                {
                    return
                    [
                        new Result
                        {
                            Title = "No packages found",
                            SubTitle = GetNoResultsMessage(parsedQuery),
                            IcoPath = IconPath,
                            Score = 0
                        }
                    ];
                }

                // Convert packages to PowerToys Run results
                return packages.Select((pkg, index) => CreateResult(pkg, index)).ToList();
            }
            catch (OperationCanceledException)
            {
                Log.Info("Search was cancelled", GetType());
                return [];
            }
            catch (Exception ex)
            {
                Log.Exception("Error processing query", ex, GetType());
                return
                [
                    new Result
                    {
                        Title = "Error",
                        SubTitle = "An error occurred while searching. Please try again.",
                        IcoPath = IconPath
                    }
                ];
            }
        }

        private Result CreateResult(PackageInfo package, int index)
        {
            return new Result
            {
                Title = $"{package.Name}",
                SubTitle = $"{package.GetRegistryName()} • v{package.Version} • {TrimDescription(package.Description)} • {package.Author}",
                IcoPath = GetPackageIcon(package.Registry),
                Score = 1000 - index,
                ToolTipData = new ToolTipData(
                    package.Name,
                    $"Registry: {package.GetRegistryName()}\nVersion: {package.Version}\nAuthor: {package.Author}\nDescription: {package.Description}\n\nInstall: {package.InstallCommand}"
                ),
                Action = _ =>
                {
                    // Default action: Copy install command to clipboard
                    return CopyToClipboard(package.InstallCommand);
                },
                ContextData = package
            };
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult?.ContextData is not PackageInfo package)
            {
                return [];
            }

            return
            [
                new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Copy install command (Enter)",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE8C8", // Copy
                    AcceleratorKey = Key.Enter,
                    Action = _ => CopyToClipboard(package.InstallCommand),
                },
                new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Copy package name (Ctrl+C)",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE8C8", // Copy
                    AcceleratorKey = Key.C,
                    AcceleratorModifiers = ModifierKeys.Control,
                    Action = _ => CopyToClipboard(package.Name),
                },
                new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Open package page (Ctrl+O)",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE8A7", // OpenInNewWindow
                    AcceleratorKey = Key.O,
                    AcceleratorModifiers = ModifierKeys.Control,
                    Action = _ => OpenUrl(package.Url),
                },
                new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Copy package URL (Ctrl+U)",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE71B", // Link
                    AcceleratorKey = Key.U,
                    AcceleratorModifiers = ModifierKeys.Control,
                    Action = _ => CopyToClipboard(package.Url),
                }
            ];
        }

        private List<Result> GetDefaultResults()
        {
            return
            [
                new Result
                {
                    Title = "Package Manager Search",
                    SubTitle = "Type a package name to search NPM, NuGet, and PyPI",
                    IcoPath = IconPath,
                    Score = 100
                },
                new Result
                {
                    Title = "Search All Registries",
                    SubTitle = "pm <package> → searches NPM, NuGet, PyPI (e.g., pm react)",
                    IcoPath = IconPath,
                    Score = 95
                },
                new Result
                {
                    Title = "Filter by Registry",
                    SubTitle = "pm npm <package> | pm pip <package> | pm nuget <package>",
                    IcoPath = IconPath,
                    Score = 90
                },
                new Result
                {
                    Title = "Supported Registries",
                    SubTitle = "NPM (Node.js), NuGet (.NET), PyPI (Python)",
                    IcoPath = IconPath,
                    Score = 85
                }
            ];
        }

        private List<Result> GetFilteredHelpResults(QueryParser parsedQuery)
        {
            var registryName = parsedQuery.GetRegistryDisplayName();
            var examples = parsedQuery.TargetRegistry switch
            {
                PackageRegistry.NPM => "pm npm react | pm npm express | pm npm lodash",
                PackageRegistry.NuGet => "pm nuget entity | pm nuget newtonsoft | pm nuget automapper",
                PackageRegistry.PyPI => "pm pip django | pm pip requests | pm pip flask",
                _ => "pm <package-name>"
            };

            return
            [
                new Result
                {
                    Title = $"Search {registryName}",
                    SubTitle = $"Type a package name to search in {registryName}",
                    IcoPath = IconPath,
                    Score = 100
                },
                new Result
                {
                    Title = "Examples",
                    SubTitle = examples,
                    IcoPath = IconPath,
                    Score = 90
                }
            ];
        }

        private string GetNoResultsMessage(QueryParser parsedQuery)
        {
            if (parsedQuery.IsFiltered)
            {
                return $"No packages matching '{parsedQuery.SearchTerm}' in {parsedQuery.GetRegistryDisplayName()}";
            }
            else
            {
                return $"No packages matching '{parsedQuery.SearchTerm}' in NPM, NuGet, or PyPI";
            }
        }

        private string TrimDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return "No description";
            var trimmed = description.Trim();
            return trimmed.Length > 80 ? trimmed.Substring(0, 77) + "…" : trimmed;
        }

        private string GetPackageIcon(PackageRegistry registry)
        {
            var theme = Context.API.GetCurrentTheme();
            var isDark = theme == Theme.Dark || theme == Theme.HighContrastBlack;
            var suffix = isDark ? "dark" : "light";

            return registry switch
            {
                PackageRegistry.NPM => $"Images/npm.{suffix}.png",
                PackageRegistry.NuGet => $"Images/nuget.{suffix}.png",
                PackageRegistry.PyPI => $"Images/pypi.{suffix}.png",
                _ => IconPath
            };
        }

        private bool CopyToClipboard(string text)
        {
            try
            {
                Clipboard.SetDataObject(text);
                Log.Info($"Copied to clipboard: {text}", GetType());
                return true;
            }
            catch (Exception ex)
            {
                Log.Exception("Failed to copy to clipboard", ex, GetType());
                return false;
            }
        }

        private bool OpenUrl(string url)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
                Log.Info($"Opened URL: {url}", GetType());
                return true;
            }
            catch (Exception ex)
            {
                Log.Exception($"Failed to open URL: {url}", ex, GetType());
                return false;
            }
        }

        private void UpdateIconPath(Theme theme)
        {
            IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite
                ? "Images/packagemanager.light.png"
                : "Images/packagemanager.dark.png";
        }

        private void OnThemeChanged(Theme currentTheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events from the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }

            _searchCancellation?.Cancel();
            _searchCancellation?.Dispose();
            _searchService?.Dispose();

            Disposed = true;
        }
    }
}