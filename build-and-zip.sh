#!/bin/bash
set -e

# ===== CONFIG =====
ROOT_DIR="$(pwd)"
PROJECT_PATH="PackageManager/Community.PowerToys.Run.Plugin.PackageManager/Community.PowerToys.Run.Plugin.PackageManager.csproj"
PLUGIN_NAME="PackageManager"
PUBLISH_DIR="PackageManager/Publish"

# ===== CLEAN UP =====
rm -rf "$PUBLISH_DIR"
rm -rf "PackageManager/Community.PowerToys.Run.Plugin.PackageManager/bin"
rm -rf "PackageManager/Community.PowerToys.Run.Plugin.PackageManager/obj"
rm -f "${ROOT_DIR}/${PLUGIN_NAME}-"*.zip

# ===== GET VERSION =====
VERSION=$(grep '"Version"' PackageManager/Community.PowerToys.Run.Plugin.PackageManager/plugin.json | sed 's/.*"Version": "\([^"]*\)".*/\1/')
echo "📋 Plugin: $PLUGIN_NAME"
echo "📋 Version: $VERSION"

# ===== DEPENDENCIES TO EXCLUDE =====
DEPENDENCIES_TO_EXCLUDE="PowerToys.Common.UI.* PowerToys.ManagedCommon.* PowerToys.Settings.UI.Lib.* Wox.Infrastructure.* Wox.Plugin.*"

# ===== BUILD X64 =====
echo "🛠️  Building for x64..."
dotnet publish "$PROJECT_PATH" -c Release -r win-x64 -p:Platform=x64 -p:TargetPlatform=windows -p:UseAppHost=false --self-contained false

# ===== BUILD ARM64 =====
echo "🛠️  Building for ARM64..."
dotnet publish "$PROJECT_PATH" -c Release -r win-arm64 -p:Platform=ARM64 -p:TargetPlatform=windows -p:UseAppHost=false --self-contained false

# ===== PACKAGE FUNCTION =====
package_build() {
    ARCH=$1
    CLEAN_ARCH="${ARCH#win-}" # remove 'win-' prefix
    # Platform needs to match what dotnet build uses (x64 vs X64, ARM64 vs arm64)
    if [ "$CLEAN_ARCH" = "arm64" ]; then
        PLATFORM_DIR="ARM64"
    else
        PLATFORM_DIR="$CLEAN_ARCH"
    fi
    echo "📦 Packaging $CLEAN_ARCH..."

    BUILD_PATH="./PackageManager/Community.PowerToys.Run.Plugin.PackageManager/bin/$PLATFORM_DIR/Release/net9.0-windows10.0.22621.0/$ARCH/publish"
    STAGE_DIR="./PackageManager/Publish/$CLEAN_ARCH"
    DEST="$STAGE_DIR/$PLUGIN_NAME"
    ZIP_PATH="${ROOT_DIR}/${PLUGIN_NAME}-${VERSION}-${CLEAN_ARCH}.zip"

    rm -rf "$STAGE_DIR"
    mkdir -p "$DEST"
    cp -r "$BUILD_PATH"/* "$DEST/"

    # Ensure plugin.json and Images are included
    if [ ! -f "$DEST/plugin.json" ]; then
        echo "⚠️  Warning: plugin.json not found in publish output, copying manually..."
        cp "./PackageManager/Community.PowerToys.Run.Plugin.PackageManager/plugin.json" "$DEST/"
    fi
    if [ ! -d "$DEST/Images" ]; then
        echo "⚠️  Warning: Images folder not found in publish output, copying manually..."
        cp -r "./PackageManager/Community.PowerToys.Run.Plugin.PackageManager/Images" "$DEST/" 2>/dev/null || true
    fi

    # Remove PowerToys dependencies
    for dep in $DEPENDENCIES_TO_EXCLUDE; do
        find "$DEST" -name "$dep" -delete 2>/dev/null || true
    done

    # Remove unnecessary Windows dependencies
    rm -f "$DEST/Microsoft.Windows.SDK.NET.dll"
    rm -f "$DEST/WinRT.Runtime.dll"
    rm -f "$DEST/System.Text.Json.dll"
    # System.Net.Http.Json might be provided by PowerToys, but let's keep it for now as it's needed for HTTP requests

    # Create zip with proper folder structure
    (cd "$STAGE_DIR" && zip -r "$ZIP_PATH" "$PLUGIN_NAME")
    echo "✅ Created: $(basename "$ZIP_PATH")"
}

# ===== PACKAGE BUILDS =====
package_build "win-x64"
package_build "win-arm64"

# ===== CHECKSUMS =====
echo "🔐 Generating checksums..."
for file in "${PLUGIN_NAME}-${VERSION}-"*.zip; do
    echo "$(basename "$file"): $(sha256sum "$file" | cut -d' ' -f1)"
done

echo "🎉 Done! ZIP files saved in root directory:"
ls -lh "${PLUGIN_NAME}-${VERSION}-"*.zip
