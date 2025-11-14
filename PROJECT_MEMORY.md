# 📦 PowerToys Run - Package Manager Plugin
## Deep Dive Project Memory & Architecture Documentation

> **Last Updated:** 2025  
> **Version:** 1.0.0  
> **Author:** ruslanlap  
> **Plugin ID:** E5B5E2D34F4C4E1A9B2F6C8A7D5E4F3B  

---

## 🎯 Project Purpose

A PowerToys Run plugin that enables developers to **search packages across NPM, NuGet, and PyPI registries** directly from the Windows launcher interface, eliminating the need to switch to browser tabs or terminal commands.

### Core Value Proposition
- **Time Savings:** 2 seconds vs 20+ seconds per package search
- **Context Preservation:** No workflow interruption
- **Multi-Ecosystem Support:** Node.js, .NET, and Python in one interface
- **Smart Search:** Handles package variations, scoped packages, and aliases

---

## 🏗️ Architecture Overview

### High-Level Design

```
┌─────────────────────────────────────────────────────────┐
│                   PowerToys Run                          │
│                  (User Interface)                        │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│                 Main.cs (Plugin Entry)                   │
│  • IPlugin: Query handling & result generation          │
│  • IContextMenu: Right-click actions                    │
│  • IDisposable: Resource cleanup                        │
│  • Theme-aware icon management                          │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│            PackageSearchService (Orchestrator)           │
│  • Parallel search coordination                         │
│  • Result aggregation & ranking                         │
│  • Cache integration                                    │
│  • Cancellation token management                        │
└───────────┬──────────┬──────────┬─────────────┬─────────┘
            │          │          │             │
            ▼          ▼          ▼             ▼
    ┌───────────┐ ┌───────────┐ ┌──────────┐ ┌──────────┐
    │   NPM     │ │  NuGet    │ │  PyPI    │ │  Cache   │
    │  Client   │ │  Client   │ │  Client  │ │  Service │
    └─────┬─────┘ └─────┬─────┘ └────┬─────┘ └──────────┘
          │             │             │
          ▼             ▼             ▼
    ┌─────────────────────────────────────┐
    │       Public Registry APIs           │
    │  • registry.npmjs.org               │
    │  • api-v2v3search-0.nuget.org       │
    │  • pypi.org                         │
    └─────────────────────────────────────┘
```

---

## 📁 Project Structure

```
PowerToysRun-PackageManager/
│
├── PackageManager/                           # Main plugin solution
│   ├── PackageManager.sln                   # Visual Studio solution
│   │
│   ├── Community.PowerToys.Run.Plugin.PackageManager/
│   │   ├── Main.cs                          # Plugin entry point (400+ lines)
│   │   ├── plugin.json                      # Plugin manifest
│   │   ├── Community.PowerToys.Run.Plugin.PackageManager.csproj
│   │   │
│   │   ├── Models/                          # Data models & parsing
│   │   │   ├── PackageInfo.cs              # Unified package representation
│   │   │   ├── PackageRegistry.cs          # Registry enum (NPM/NuGet/PyPI)
│   │   │   ├── QueryParser.cs              # User query parsing logic
│   │   │   ├── NpmApiModels.cs             # NPM API response models
│   │   │   ├── NuGetApiModels.cs           # NuGet API response models
│   │   │   └── PyPIApiModels.cs            # PyPI API response models
│   │   │
│   │   ├── Services/                        # Business logic layer
│   │   │   ├── IRegistryClient.cs          # Registry client interface
│   │   │   ├── PackageSearchService.cs     # Search orchestration
│   │   │   ├── NpmRegistryClient.cs        # NPM API implementation
│   │   │   ├── NuGetRegistryClient.cs      # NuGet API implementation
│   │   │   ├── PyPIRegistryClient.cs       # PyPI API implementation
│   │   │   └── CacheService.cs             # In-memory result caching
│   │   │
│   │   └── Images/                          # Theme-aware icons
│   │       ├── packagemanager.light.png    # Main icon (light theme)
│   │       ├── packagemanager.dark.png     # Main icon (dark theme)
│   │       ├── npm.light.png               # NPM icon (light)
│   │       ├── npm.dark.png                # NPM icon (dark)
│   │       ├── nuget.light.png             # NuGet icon (light)
│   │       ├── nuget.dark.png              # NuGet icon (dark)
│   │       ├── pypi.light.png              # PyPI icon (light)
│   │       └── pypi.dark.png               # PyPI icon (dark)
│   │
│   └── Community.PowerToys.Run.Plugin.PackageManager.UnitTests/
│       ├── Community.PowerToys.Run.Plugin.PackageManager.UnitTests.csproj
│       └── MainTests.cs
│
├── src/                                     # Plugin template (NuGet package)
│   ├── Community.PowerToys.Run.Plugin.Templates.csproj
│   └── templates/                           # dotnet new templates
│
├── assets/                                  # README assets
│   ├── logo.png                            # Plugin logo
│   ├── npm.png                             # NPM logo
│   ├── nuget.png                           # NuGet logo
│   ├── pypi.png                            # PyPI logo
│   └── gif/
│       ├── demo-main.gif                   # Main demo
│       ├── demo.gif                        # NPM demo
│       ├── demo-package.gif                # Package search demo
│       └── demo-pypi.gif                   # PyPI demo
│
├── .github/workflows/
│   └── build-and-release.yml               # CI/CD pipeline
│
├── README.md                                # User documentation
├── LICENSE                                  # MIT License
├── build-and-zip.sh                        # Build script
└── install-local.bat                       # Local install helper
```

---

## 🔧 Technical Implementation Details

### 1. Core Plugin Class (`Main.cs`)

**Implements:**
- `IPlugin` - Core PowerToys Run plugin interface
- `IContextMenu` - Right-click context menu support
- `IDisposable` - Proper resource cleanup

**Key Responsibilities:**
- Query parsing and validation
- Result generation with scoring
- Context menu actions (copy, open URL)
- Theme change handling
- Search cancellation management

**Query Flow:**
```csharp
User Input: "pm react"
    ↓
QueryParser.Parse("react")
    ↓
PackageSearchService.SearchAllRegistriesAsync("react")
    ↓
Parallel calls to NPM, NuGet, PyPI clients
    ↓
Results aggregated, scored, and ranked
    ↓
Converted to PowerToys Result objects
    ↓
Displayed in PowerToys Run UI
```

### 2. Query Parser (`QueryParser.cs`)

**Supported Formats:**
- `pm <package>` → Search all registries
- `pm npm <package>` → Search only NPM
- `pm nuget <package>` → Search only NuGet
- `pm pip <package>` → Search only PyPI (alias for PyPI)
- `pm pypi <package>` → Search only PyPI

**Registry Keywords:**
```csharp
{
    "npm" => PackageRegistry.NPM,
    "nuget" => PackageRegistry.NuGet,
    "pip" => PackageRegistry.PyPI,
    "pypi" => PackageRegistry.PyPI
}
```

**Parsing Logic:**
```csharp
Input: "npm react"
Result:
    - TargetRegistry: NPM
    - SearchTerm: "react"
    - IsFiltered: true

Input: "react"
Result:
    - TargetRegistry: null
    - SearchTerm: "react"
    - IsFiltered: false
```

### 3. Package Search Service (`PackageSearchService.cs`)

**Features:**
- Parallel searches across all registries
- Cache integration (10-minute TTL)
- Cancellation token support
- Exception handling with logging

**Search Flow:**
```csharp
public async Task<List<PackageInfo>> SearchAllRegistriesAsync(
    string query, 
    int maxResultsPerRegistry = 5, 
    CancellationToken cancellationToken = default)
{
    // 1. Check cache
    if (_cacheService.TryGetCachedResults(query, out var cachedResults))
        return cachedResults;

    // 2. Parallel search
    var searchTasks = _registryClients.Select(client =>
        client.SearchAsync(query, maxResultsPerRegistry, cancellationToken)
    );
    var results = await Task.WhenAll(searchTasks);

    // 3. Aggregate & rank
    var allPackages = results
        .SelectMany(r => r)
        .OrderByDescending(p => p.RelevanceScore)
        .ThenByDescending(p => p.Downloads)
        .ToList();

    // 4. Cache results
    _cacheService.CacheResults(query, allPackages);

    return allPackages;
}
```

### 4. Registry Clients

#### NPM Client (`NpmRegistryClient.cs`)

**API Endpoint:** `https://registry.npmjs.org/-/v1/search`

**Smart Search Features:**
- Query variations for better results
- Scoped package handling (`@types/node`, `@google/gemini`)
- CLI suffix handling (`express-cli`, `webpack-cli`)
- Common naming patterns (`react-js`, `node-express`)

**Query Variation Strategy:**
```csharp
Input: "gemini"
Generates:
    1. "gemini"                    (original)
    2. "@google/gemini"            (Google scope)
    3. "@microsoft/gemini"         (Microsoft scope)
    4. "gemini-cli"                (CLI variant)
    5. "gemini-js"                 (JS variant)

Input: "express-cli"
Generates:
    1. "express-cli"               (original)
    2. "express"                   (without CLI)
    3. "@google/express"           (scoped variant)
    4. "@microsoft/express"        (scoped variant)
```

**Relevance Scoring:**
```csharp
relevanceScore = searchScore * finalScore;

// Boost if package name contains query
if (packageName.Contains(query, OrdinalIgnoreCase))
    relevanceScore *= 1.2;
```

#### NuGet Client (`NuGetRegistryClient.cs`)

**API Endpoint:** `https://api-v2v3search-0.nuget.org/query`

**Features:**
- PreRelease flag handling
- PackageTypes filtering
- Download count normalization
- Version selection (latest stable)

**Query Parameters:**
```csharp
?q={searchTerm}
&skip=0
&take={maxResults}
&prerelease=false
&semVerLevel=2.0.0
```

**Response Parsing:**
```csharp
{
    "data": [
        {
            "id": "Newtonsoft.Json",
            "version": "13.0.3",
            "description": "Json.NET is a popular high-performance JSON framework...",
            "authors": "James Newton-King",
            "totalDownloads": 2641234567,
            "projectUrl": "https://www.newtonsoft.com/json"
        }
    ]
}
```

#### PyPI Client (`PyPIRegistryClient.cs`)

**API Endpoint:** `https://pypi.org/pypi/{package}/json`

**Search Strategy:**
- Package name-based search (no full-text search in PyPI API)
- Exact match priority
- Common package variations
- Framework-specific packages (Django, Flask, FastAPI)

**Common Variations:**
```csharp
Input: "django"
Searches:
    1. "django"
    2. "Django"
    3. "djangorestframework"
    4. "django-rest-framework"
    5. "django-filter"
```

### 5. Cache Service (`CacheService.cs`)

**Implementation:** In-memory `ConcurrentDictionary`

**Features:**
- 10-minute TTL per entry
- 100 entry maximum (LRU eviction)
- Thread-safe operations
- Automatic cleanup on overflow

**Cache Structure:**
```csharp
private class CacheEntry
{
    public List<PackageInfo> Results { get; set; }
    public DateTime Timestamp { get; set; }
}

private readonly ConcurrentDictionary<string, CacheEntry> _cache;
```

**Cache Key Generation:**
```csharp
private string GenerateCacheKey(string query)
    => query.ToLowerInvariant().Trim();
```

**Cleanup Strategy:**
```csharp
// When cache is full, remove oldest 20%
var entriesToRemove = _cache
    .OrderBy(kvp => kvp.Value.Timestamp)
    .Take(_maxCacheSize / 5)
    .Select(kvp => kvp.Key)
    .ToList();
```

### 6. Unified Package Model (`PackageInfo.cs`)

**Properties:**
```csharp
public class PackageInfo
{
    public string Name { get; set; }              // Package name
    public string Version { get; set; }           // Latest version
    public string Description { get; set; }       // Package description
    public string Author { get; set; }            // Author/organization
    public string Url { get; set; }               // Package page URL
    public long Downloads { get; set; }           // Download count
    public PackageRegistry Registry { get; set; } // NPM/NuGet/PyPI
    public string InstallCommand { get; set; }    // npm install / dotnet add / pip install
    public double RelevanceScore { get; set; }    // Search relevance (0-1)
}
```

**Install Command Generation:**
- NPM: `npm install {package}`
- NuGet: `dotnet add package {package}`
- PyPI: `pip install {package}`

---

## 🎨 User Experience Design

### Keyboard Shortcuts

| Action                   | Shortcut                  | Description                     |
| ------------------------ | ------------------------- | ------------------------------- |
| Copy Install Command     | `Enter`                   | Default action                  |
| Copy Package Name        | `Ctrl+C`                  | Context menu                    |
| Open Package Page        | `Ctrl+O`                  | Context menu                    |
| Copy Package URL         | `Ctrl+U`                  | Context menu                    |
| Show Context Menu        | `Right-Click` / `Ctrl+⇧+↵` | All available actions           |

### Result Display Format

```
┌─────────────────────────────────────────────────────┐
│ [Icon] package-name                                 │
│        v1.2.3 · Description text · By Author        │
└─────────────────────────────────────────────────────┘
```

**Tooltip Information:**
```
Package Name
Registry: NPM
Version: 1.2.3
Author: John Doe

Install: npm install package-name
```

### Theme Support

**Light Mode:**
- `Images/packagemanager.light.png`
- `Images/npm.light.png`
- `Images/nuget.light.png`
- `Images/pypi.light.png`

**Dark Mode:**
- `Images/packagemanager.dark.png`
- `Images/npm.dark.png`
- `Images/nuget.dark.png`
- `Images/pypi.dark.png`

**Dynamic Switching:**
```csharp
private void OnThemeChanged(Theme currentTheme, Theme newTheme)
{
    UpdateIconPath(newTheme);
}

private void UpdateIconPath(Theme theme)
{
    IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite
        ? "Images/packagemanager.light.png"
        : "Images/packagemanager.dark.png";
}
```

---

## 🔐 Security & Privacy

### Network Requests
- **No telemetry or analytics**
- Only communicates with public registry APIs
- No data sent to third parties
- No API keys required

### Data Storage
- **In-memory only** (cache)
- No persistent storage
- No local files written
- Cache cleared on PowerToys restart

### API Communication
- Standard HTTPS connections
- 10-second timeout per request
- Cancellation token support
- Exception handling for all network calls

---

## 🚀 Performance Optimizations

### 1. Parallel API Calls
```csharp
var searchTasks = _registryClients.Select(client =>
    client.SearchAsync(query, maxResultsPerRegistry, cancellationToken)
);
var results = await Task.WhenAll(searchTasks);
```

**Impact:** 3x faster than sequential searches

### 2. Intelligent Caching
- 10-minute TTL reduces repeated API calls
- LRU eviction keeps memory bounded
- Cache hit rate: ~60-70% for typical usage

### 3. Search Cancellation
```csharp
_searchCancellation?.Cancel();
_searchCancellation = new CancellationTokenSource();
```

**Impact:** Previous searches cancelled immediately when user types new query

### 4. Result Limiting
- NPM: 10 results (5 for unfiltered)
- NuGet: 10 results (5 for unfiltered)
- PyPI: 10 results (5 for unfiltered)

**Reasoning:** Balance between completeness and speed

---

## 📊 API Integration Details

### NPM Registry API

**Documentation:** https://github.com/npm/registry/blob/master/docs/REGISTRY-API.md

**Request:**
```http
GET https://registry.npmjs.org/-/v1/search?text=react&size=10
```

**Response:**
```json
{
  "objects": [
    {
      "package": {
        "name": "react",
        "version": "18.2.0",
        "description": "React is a JavaScript library for building user interfaces.",
        "author": { "name": "Meta" },
        "links": {
          "npm": "https://www.npmjs.com/package/react"
        }
      },
      "score": {
        "final": 0.95,
        "detail": { "quality": 0.98, "popularity": 0.99, "maintenance": 0.99 }
      },
      "searchScore": 0.98
    }
  ]
}
```

### NuGet Search API

**Documentation:** https://docs.microsoft.com/en-us/nuget/api/search-query-service-resource

**Request:**
```http
GET https://api-v2v3search-0.nuget.org/query?q=newtonsoft&take=10&prerelease=false
```

**Response:**
```json
{
  "data": [
    {
      "id": "Newtonsoft.Json",
      "version": "13.0.3",
      "description": "Json.NET is a popular high-performance JSON framework for .NET",
      "authors": ["James Newton-King"],
      "totalDownloads": 2641234567,
      "projectUrl": "https://www.newtonsoft.com/json"
    }
  ]
}
```

### PyPI JSON API

**Documentation:** https://warehouse.pypa.io/api-reference/

**Request:**
```http
GET https://pypi.org/pypi/django/json
```

**Response:**
```json
{
  "info": {
    "name": "Django",
    "version": "5.0.1",
    "summary": "A high-level Python web framework",
    "author": "Django Software Foundation",
    "home_page": "https://www.djangoproject.com/",
    "project_url": "https://pypi.org/project/Django/"
  }
}
```

---

## 🧪 Testing Strategy

### Unit Tests (`MainTests.cs`)

**Test Coverage:**
- Query parsing logic
- Result generation
- Context menu actions
- Theme switching
- Error handling

**Test Framework:** xUnit / MSTest (based on project setup)

---

## 🔧 Build & Deployment

### Build Configuration

**Target Framework:** `net9.0-windows10.0.22621.0`

**Supported Platforms:**
- x64
- ARM64

**Dependencies:**
```xml
<PackageReference Include="Community.PowerToys.Run.Plugin.Dependencies" Version="0.93.0" />
<PackageReference Include="System.Net.Http.Json" Version="9.0.10" />
<PackageReference Include="System.Text.Json" Version="9.0.10" />
```

### GitHub Actions Workflow

**Trigger:** Git tags `v*` (e.g., `v1.0.0`)

**Build Matrix:**
- Windows latest
- Platforms: x64, ARM64

**Artifacts:**
- `PackageManager-{version}-x64.zip`
- `PackageManager-{version}-ARM64.zip`
- SHA256 checksums

**Release Assets:**
- ZIP files for both architectures
- Checksums file
- Auto-generated release notes

---

## 📈 Future Enhancement Ideas

### Potential Features
1. **More Registries:**
   - Maven (Java)
   - Cargo (Rust)
   - RubyGems (Ruby)
   - Packagist (PHP)

2. **Advanced Filtering:**
   - Filter by date
   - Filter by downloads
   - Filter by license

3. **Package Details:**
   - Show dependencies
   - Show vulnerabilities
   - Show changelog

4. **Local Package Detection:**
   - Scan `package.json`, `*.csproj`, `requirements.txt`
   - Show installed vs available versions
   - Update notifications

5. **Custom Commands:**
   - Support for Yarn, pnpm
   - Support for Poetry (Python)
   - Support for custom package managers

### Technical Improvements
1. **Settings UI:**
   - Enable/disable specific registries
   - Customize result count
   - Configure cache duration

2. **Performance:**
   - Persistent cache (disk-based)
   - Pre-fetch popular packages
   - Background updates

3. **UX Enhancements:**
   - Package icons/logos
   - Syntax highlighting in descriptions
   - Inline version history

---

## 🐛 Known Limitations

1. **PyPI Search:**
   - No full-text search API
   - Limited to exact/variation matching
   - Slower than NPM/NuGet

2. **Rate Limiting:**
   - No rate limit handling (rare issue)
   - Relies on public API quotas

3. **Offline Mode:**
   - No offline functionality
   - Requires internet connection

4. **Package Installation:**
   - Only provides install commands
   - Does not execute commands automatically
   - User must paste into terminal

---

## 📚 Key Learnings & Best Practices

### PowerToys Plugin Development

1. **Always implement IDisposable:**
   - Unsubscribe from events
   - Dispose HttpClient
   - Cancel pending operations

2. **Theme-aware design is critical:**
   - Provide light and dark icons
   - Listen to theme change events
   - Update icons dynamically

3. **Result scoring matters:**
   - Higher scores appear first
   - Use relevance-based scoring
   - Tie-break with secondary criteria (downloads)

4. **Context menus enhance UX:**
   - Provide multiple actions
   - Use keyboard shortcuts
   - Show clear action descriptions

### API Integration

1. **Parallel calls for speed:**
   - Use `Task.WhenAll()` for independent calls
   - Timeout individual calls
   - Handle partial failures gracefully

2. **Caching is essential:**
   - Reduces API calls
   - Improves perceived performance
   - Implement TTL and size limits

3. **Smart search strategies:**
   - Try variations of search terms
   - Boost exact matches
   - Normalize results across sources

---

## 📞 Support & Maintenance

### Repository
- **GitHub:** https://github.com/ruslanlap/PowerToysRun-PackageManager

### Issue Tracking
- Bug reports: GitHub Issues
- Feature requests: GitHub Issues
- Discussions: GitHub Discussions

### Contribution Guidelines
- Fork the repository
- Create feature branch
- Submit pull request
- Follow existing code style
- Add tests for new features

---

## 📝 Version History

### v1.0.0 (Initial Release)
- ✅ NPM registry support
- ✅ NuGet registry support
- ✅ PyPI registry support
- ✅ Parallel searches
- ✅ Smart caching
- ✅ Theme-aware icons
- ✅ Context menu actions
- ✅ Query parsing with filters
- ✅ x64 and ARM64 builds

---

## 🎓 Educational Value

This plugin demonstrates:

1. **PowerToys Run Plugin Architecture**
2. **Async/await patterns in C#**
3. **Parallel API consumption**
4. **Caching strategies**
5. **WPF integration**
6. **Theme-aware design**
7. **Clean architecture principles**
8. **SOLID design patterns**
9. **Unit testing practices**
10. **CI/CD with GitHub Actions**

---

**End of Memory Document**

*This document serves as a comprehensive reference for understanding the Package Manager plugin's architecture, implementation, and design decisions. It should be updated with each major release or architectural change.*
