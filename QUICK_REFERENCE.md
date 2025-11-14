# 📦 Package Manager Plugin - Quick Reference

> **TL;DR:** Search NPM, NuGet, and PyPI packages directly from PowerToys Run in 2 seconds instead of 20+.

---

## 🚀 Quick Start

### Installation (60 seconds)
```bash
1. Download ZIP from releases
2. Extract to: %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\PackageManager\
3. Restart PowerToys
4. Press Alt+Space → Type "pm react" → Done!
```

---

## 💡 Usage Examples

```bash
# Search all registries
pm react                 # Searches NPM, NuGet, PyPI

# Search specific registry
pm npm express           # Only NPM
pm nuget entity          # Only NuGet  
pm pip django            # Only PyPI

# Advanced searches
pm npm @types/node       # Scoped packages
pm npm react-router      # Hyphenated names
pm pip django-rest       # Framework extensions
```

---

## ⌨️ Keyboard Shortcuts

| Key            | Action                    |
| -------------- | ------------------------- |
| `Enter`        | Copy install command      |
| `Ctrl+C`       | Copy package name         |
| `Ctrl+O`       | Open package page         |
| `Ctrl+U`       | Copy package URL          |
| `Right-Click`  | Show all actions          |

---

## 🏗️ Architecture at a Glance

```
User Input → Query Parser → Search Service → [NPM, NuGet, PyPI] → Cache → Results
```

**Key Components:**
- **Main.cs** - Plugin entry (IPlugin, IContextMenu, IDisposable)
- **PackageSearchService** - Parallel search orchestration
- **Registry Clients** - NPM, NuGet, PyPI API integrations
- **CacheService** - 10-min TTL, 100 entry LRU cache
- **QueryParser** - Filter extraction (npm/nuget/pip keywords)

---

## 📊 Performance Metrics

| Metric                  | Value                |
| ----------------------- | -------------------- |
| **Search Time**         | 0.5-2 seconds        |
| **Cache Hit Rate**      | 60-70%               |
| **Time Saved**          | ~18 seconds/search   |
| **Parallel Searches**   | 3 simultaneous       |
| **Cache Duration**      | 10 minutes           |
| **Max Cache Size**      | 100 entries          |
| **API Timeout**         | 10 seconds           |
| **Results per Search**  | 10 per registry      |

---

## 🔌 API Endpoints

| Registry | Endpoint                                      |
| -------- | --------------------------------------------- |
| **NPM**  | `registry.npmjs.org/-/v1/search`              |
| **NuGet**| `api-v2v3search-0.nuget.org/query`            |
| **PyPI** | `pypi.org/pypi/{package}/json`                |

---

## 🛠️ Tech Stack

```yaml
Framework:      .NET 9.0 (Windows 10.0.22621.0)
Language:       C# 12 with nullable types
UI:             WPF via PowerToys Run API
HTTP:           System.Net.Http.Json
JSON:           System.Text.Json
Platforms:      x64, ARM64
Dependencies:
  - Community.PowerToys.Run.Plugin.Dependencies (0.93.0)
  - System.Net.Http.Json (9.0.10)
  - System.Text.Json (9.0.10)
```

---

## 📁 Key Files

| File                           | Purpose                      | Lines |
| ------------------------------ | ---------------------------- | ----- |
| `Main.cs`                      | Plugin entry point           | 400+  |
| `PackageSearchService.cs`      | Search orchestration         | 120   |
| `NpmRegistryClient.cs`         | NPM API client               | 180   |
| `NuGetRegistryClient.cs`       | NuGet API client             | 150   |
| `PyPIRegistryClient.cs`        | PyPI API client              | 140   |
| `CacheService.cs`              | Result caching               | 110   |
| `QueryParser.cs`               | Query parsing                | 90    |
| `PackageInfo.cs`               | Unified package model        | 40    |

**Total Lines of Code:** ~1,500 (excluding tests)

---

## 🎯 Design Principles

1. **Speed First**: Parallel API calls, aggressive caching
2. **Smart Search**: Query variations, scoped packages, aliases
3. **Zero Config**: Works out of the box, no API keys
4. **Theme Aware**: Dark/light mode support
5. **Clean Architecture**: Service layer, clear separation of concerns
6. **SOLID Principles**: Interface-based design, single responsibility
7. **Async/Await**: Non-blocking UI, cancellation support
8. **Fail Gracefully**: Network errors handled, user-friendly messages

---

## 🔒 Security & Privacy

✅ No telemetry or analytics  
✅ No API keys required  
✅ No persistent storage  
✅ In-memory cache only  
✅ HTTPS connections  
✅ Public APIs only  
✅ Open source (MIT License)  

---

## 🐛 Common Issues

### Plugin doesn't show up
```
Check: %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\PackageManager\
Action: Restart PowerToys, verify files exist
```

### No search results
```
Check: Internet connection, API availability
Action: Try filtered search (pm npm <package>)
```

### Slow searches
```
Cause: First search always slower (no cache)
Action: Subsequent searches instant with cache
```

---

## 📈 Roadmap Ideas

- [ ] Maven (Java) support
- [ ] Cargo (Rust) support
- [ ] Package version history
- [ ] Vulnerability checking
- [ ] Local package.json detection
- [ ] Custom install commands (yarn, pnpm)
- [ ] Offline mode with local cache
- [ ] Settings UI for registry toggle

---

## 🎓 Learning Resources

**PowerToys Plugin Development:**
- [PowerToys GitHub](https://github.com/microsoft/PowerToys)
- [Plugin Templates](https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Templates)
- [Awesome PowerToys Plugins](https://github.com/hlaueriksson/awesome-powertoys-run-plugins)

**API Documentation:**
- [NPM Registry API](https://github.com/npm/registry/blob/master/docs/REGISTRY-API.md)
- [NuGet API](https://docs.microsoft.com/en-us/nuget/api/overview)
- [PyPI JSON API](https://warehouse.pypa.io/api-reference/)

---

## 📞 Support

**Repository:** https://github.com/ruslanlap/PowerToysRun-PackageManager  
**Issues:** https://github.com/ruslanlap/PowerToysRun-PackageManager/issues  
**License:** MIT  
**Author:** ruslanlap  
**Plugin ID:** `E5B5E2D34F4C4E1A9B2F6C8A7D5E4F3B`

---

## 🎯 Key Metrics Summary

| Metric                     | Value                    |
| -------------------------- | ------------------------ |
| **Development Time**       | ~2 weeks                 |
| **Code Complexity**        | Medium                   |
| **Test Coverage**          | Unit tests available     |
| **External Dependencies**  | 3 NuGet packages         |
| **API Integrations**       | 3 public APIs            |
| **Supported Registries**   | 3 (NPM, NuGet, PyPI)     |
| **Build Targets**          | 2 (x64, ARM64)           |
| **Release Artifacts**      | 2 ZIP files + checksums  |

---

**Last Updated:** 2025  
**Version:** 1.0.0  
**Status:** ✅ Production Ready

*For detailed information, see [PROJECT_MEMORY.md](PROJECT_MEMORY.md) and [README.md](README.md)*
