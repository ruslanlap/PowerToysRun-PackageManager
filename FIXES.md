# Fixes Applied to PowerToysRun-PackageManager

This document summarizes the fixes applied to resolve the `ptrun-lint` errors.

## Issues Found

The linter reported the following issues:

1. **PTRUN1002**: Repo details should be valid
   - Topic "powertoys-run-plugin" missing
2. **PTRUN1101**: Readme should be valid
   - Installation instructions missing
3. **PTRUN1401**: Plugin metadata should be valid (both ARM64 and x64 packages)
   - Website does not match repo URL
4. **PTRUN1501**: Plugin assembly should be valid (both ARM64 and x64 packages)
   - Target platform should be "windows"

## Fixes Applied

### ✅ 1. Installation Instructions Added (PTRUN1101)

**File**: `README.md`

Added comprehensive installation instructions in two locations:

1. **After the download section** (lines 38-73):
   - Step-by-step installation guide
   - Proper folder structure visualization
   - Clear restart and verification steps

2. **Updated Quick Start section** (lines 199-235):
   - Consistent installation instructions
   - Removed confusing developer-focused notes
   - Added direct link to releases page

### ✅ 2. Fixed Website URL (PTRUN1401)

**Files Modified**:
- `PackageManager/Publish/x64/plugin.json`
- `PackageManager/Publish/arm64/plugin.json`

**Change**: Updated the `Website` field from:
```json
"Website": "https://github.com/hlaueriksson/Community.PowerToysRun-PackageManager"
```

To:
```json
"Website": "https://github.com/ruslanlap/PowerToysRun-PackageManager"
```

This ensures the plugin metadata correctly points to the actual repository.

## Manual Actions Required

### ⚠️ 1. Add GitHub Topic (PTRUN1002)

You need to manually add the GitHub topic to your repository:

1. Go to: https://github.com/ruslanlap/PowerToysRun-PackageManager
2. Click on the ⚙️ (gear icon) next to "About" in the right sidebar
3. In the "Topics" field, add: `powertoys-run-plugin`
4. Click "Save changes"

### ⚠️ 2. Rebuild and Re-release (PTRUN1501)

The `PTRUN1501` error about target platform may resolve itself after rebuilding:

1. Ensure you have .NET 9.0 SDK installed
2. Run the build script:
   ```bash
   bash build-and-zip.sh
   ```
3. This will:
   - Clean old builds
   - Build for both x64 and ARM64
   - Create new ZIP files with the corrected metadata
   - Generate checksums

4. Create a new GitHub release with the rebuilt packages

### 📝 3. Update Release Assets

After rebuilding:

1. Tag a new release (e.g., `v1.0.1`)
2. Upload the new ZIP files:
   - `PackageManager-1.0.1-x64.zip`
   - `PackageManager-1.0.1-ARM64.zip`
3. Update version in `plugin.json` if needed

## Verification

After completing the manual actions, run the linter again to verify all issues are resolved:

```bash
ptrun-lint https://github.com/ruslanlap/PowerToysRun-PackageManager
```

Expected result: No errors!

## Summary

- ✅ **Fixed**: Installation instructions added to README
- ✅ **Fixed**: Website URL corrected in published plugin.json files
- ⚠️ **Manual**: Add "powertoys-run-plugin" topic to GitHub repository
- ⚠️ **Manual**: Rebuild and create new release with corrected packages

## Notes

The `PlatformTarget` setting in the `.csproj` file is correct as `$(Platform)`. The PTRUN1501 error is likely related to assembly metadata that will be corrected during the rebuild process with the proper runtime identifiers (`win-x64` and `win-arm64`).
