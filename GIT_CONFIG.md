# 🔧 Git Configuration Summary

> **Project:** PowerToys Run - Package Manager Plugin  
> **Repository:** https://github.com/ruslanlap/PowerToysRun-PackageManager  
> **Default Branch:** `master` ✅

---

## ✅ Branch Configuration

### Primary Branch
- **Branch Name:** `master`
- **Status:** Active and configured
- **Used In:**
  - GitHub Actions workflows
  - Release pipelines
  - CI/CD automation

### GitHub Actions Configuration

**File:** `.github/workflows/build-and-release.yml`

```yaml
on:
    push:
        tags:
            - "v*"
        branches:
            - master  # ✅ Correctly configured
    pull_request:
        branches:
            - master  # ✅ Correctly configured
```

---

## 📋 Configuration Checklist

### ✅ Completed Items

- [x] **GitHub Actions workflow** updated to use `master` branch
- [x] **Release workflow** triggers on `master` branch pushes
- [x] **Pull request workflow** targets `master` branch
- [x] **Build artifacts** configured for both x64 and ARM64
- [x] **Release notes** reference correct repository paths
- [x] **Environment variables** set to PackageManager plugin paths
- [x] **Artifact naming** uses `PackageManager` prefix

### 📝 Branch-Related Settings

| Setting                  | Value        | Status |
| ------------------------ | ------------ | ------ |
| Default Branch           | `master`     | ✅     |
| Protected Branch         | `master`     | ⚠️ Recommended |
| Actions Trigger Branch   | `master`     | ✅     |
| PR Target Branch         | `master`     | ✅     |
| Release Source Branch    | `master`     | ✅     |

---

## 🚀 Workflow Triggers

### Tag-based Releases
```bash
# Create and push a version tag
git tag v1.0.0
git push origin v1.0.0

# GitHub Actions will:
# 1. Build for x64 and ARM64
# 2. Create ZIP artifacts
# 3. Generate SHA256 checksums
# 4. Create GitHub Release
# 5. Upload artifacts to release
```

### Branch-based Builds
```bash
# Push to master triggers CI build
git push origin master

# Pull requests to master trigger validation
git checkout -b feature/new-feature
git push origin feature/new-feature
# Create PR targeting master
```

---

## 📦 Release Process

### 1. Version Tagging
```bash
# Ensure you're on master branch
git checkout master
git pull origin master

# Create version tag
git tag -a v1.0.0 -m "Release v1.0.0 - Initial release"
git push origin v1.0.0
```

### 2. Automated Build
GitHub Actions automatically:
- ✅ Builds plugin for x64 and ARM64
- ✅ Injects version into `plugin.json`
- ✅ Creates ZIP artifacts
- ✅ Generates checksums
- ✅ Creates GitHub Release

### 3. Release Artifacts
```
PackageManager-1.0.0-x64.zip
PackageManager-1.0.0-ARM64.zip
PackageManager-1.0.0-x64.zip.sha256
PackageManager-1.0.0-ARM64.zip.sha256
checksums.txt
```

---

## 🔐 Recommended Branch Protection

### Suggested Settings for `master` Branch

**Go to:** Repository Settings → Branches → Add Branch Protection Rule

```yaml
Branch name pattern: master

Protect matching branches:
  ✅ Require a pull request before merging
     □ Require approvals: 1
  ✅ Require status checks to pass before merging
     ✅ Require branches to be up to date before merging
     Status checks: 
       - build (x64)
       - build (ARM64)
  □ Require conversation resolution before merging
  ✅ Require linear history
  □ Require deployments to succeed before merging
  □ Lock branch (read-only)
  ✅ Do not allow bypassing the above settings
```

---

## 🛠️ Local Development

### Clone Repository
```bash
git clone https://github.com/ruslanlap/PowerToysRun-PackageManager.git
cd PowerToysRun-PackageManager
git checkout master
```

### Create Feature Branch
```bash
# Always branch from master
git checkout master
git pull origin master
git checkout -b feature/my-new-feature
```

### Submit Pull Request
```bash
# Push feature branch
git push origin feature/my-new-feature

# Create PR on GitHub targeting 'master' branch
```

---

## 📝 External References

### Third-Party Repositories (Keep As-Is)

These repositories use `main` branch (external, not under our control):

- **Microsoft FluentUI Emoji:** `https://raw.githubusercontent.com/microsoft/fluentui-emoji/main/`
- **NPM Registry Docs:** `https://github.com/npm/registry/blob/master/`

**Note:** These are external dependencies and should NOT be changed.

---

## ✅ Verification Commands

### Check Current Branch
```bash
git branch
# Output should show: * master
```

### Check Remote Configuration
```bash
git remote -v
# Origin should point to: https://github.com/ruslanlap/PowerToysRun-PackageManager.git
```

### Verify Workflow Syntax
```bash
# Check workflow file
cat .github/workflows/build-and-release.yml | grep -A 5 "branches:"

# Expected output:
#   branches:
#     - master
```

---

## 🎯 Summary

✅ **Master branch is correctly configured throughout the project**

| Component                | Configuration | Status |
| ------------------------ | ------------- | ------ |
| GitHub Actions           | `master`      | ✅     |
| Default Branch           | `master`      | ✅     |
| Documentation            | Branch-agnostic | ✅   |
| Release Pipeline         | `master`      | ✅     |
| CI/CD Triggers           | `master`      | ✅     |

**All systems configured for `master` branch! 🚀**

---

**Last Updated:** 2025  
**Configured By:** ruslanlap  
**Status:** ✅ Production Ready
