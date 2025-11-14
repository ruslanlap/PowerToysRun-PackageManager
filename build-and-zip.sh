#!/bin/bash
set -e

# ===== PERFORMANCE OPTIMIZATIONS =====
# Disable .NET telemetry and first-time experience for faster builds
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_NOLOGO=1
export MSBUILDDISABLENODEREUSE=1

# ===== CONFIG =====
ROOT_DIR="$(pwd)"
PROJECT_PATH="PackageManager/Community.PowerToys.Run.Plugin.PackageManager/Community.PowerToys.Run.Plugin.PackageManager.csproj"
PLUGIN_NAME="PackageManager"
PLUGIN_DIR="PackageManager/Community.PowerToys.Run.Plugin.PackageManager"
PUBLISH_DIR="PackageManager/Publish"

# ===== CLEAN UP =====
echo "🧹 Cleaning up previous builds..."
rm -rf "$PUBLISH_DIR"
rm -rf "$PLUGIN_DIR/bin"
rm -rf "$PLUGIN_DIR/obj"
rm -f "${ROOT_DIR}/${PLUGIN_NAME}-"*.zip

# ===== GET VERSION =====
VERSION=$(grep '"Version"' "$PLUGIN_DIR/plugin.json" | sed 's/.*"Version": "\([^"]*\)".*/\1/')
echo "📋 Plugin: $PLUGIN_NAME"
echo "📋 Version: $VERSION"
echo ""

# ===== DEPENDENCIES TO EXCLUDE =====
DEPENDENCIES_TO_EXCLUDE=("PowerToys.Common.UI.*" "PowerToys.ManagedCommon.*" "PowerToys.Settings.UI.Lib.*" "Wox.Infrastructure.*" "Wox.Plugin.*")

# ===== OPTIMIZED BUILD FUNCTION =====
build_platform() {
    local RID=$1
    local ARCH="${RID#win-}"  # Remove 'win-' prefix

    echo "🛠️  Building for $ARCH..."

    # PERFORMANCE: Optimized build parameters (from workflow)
    dotnet build "$PROJECT_PATH" \
        -c Release \
        -p:Platform="$ARCH" \
        -p:RuntimeIdentifier="$RID" \
        -p:UseSharedCompilation=false \
        -p:BuildInParallel=true \
        -p:ContinuousIntegrationBuild=true \
        -p:Deterministic=true \
        -p:DebugType=none \
        -p:DebugSymbols=false \
        --nologo \
        -v:minimal

    echo "✅ Build completed for $ARCH"
}

# ===== PARALLEL BUILDS =====
echo "⚡ Building both platforms in parallel..."
build_platform "win-x64" &
PID_X64=$!

build_platform "win-arm64" &
PID_ARM64=$!

# Wait for both builds to complete
wait $PID_X64 || { echo "❌ x64 build failed"; exit 1; }
wait $PID_ARM64 || { echo "❌ ARM64 build failed"; exit 1; }

echo ""
echo "✅ All builds completed successfully!"
echo ""

# ===== OPTIMIZED PACKAGE FUNCTION =====
package_build() {
    local RID=$1
    local ARCH="${RID#win-}"  # Remove 'win-' prefix

    echo "📦 Packaging $ARCH..."

    # PERFORMANCE: Calculate paths based on build with RuntimeIdentifier
    local BUILD_PATH="$PLUGIN_DIR/bin/$ARCH/Release/net9.0-windows10.0.22621.0/$RID"
    local STAGE_DIR="$PUBLISH_DIR/$ARCH/$PLUGIN_NAME"
    local ZIP_PATH="${ROOT_DIR}/${PLUGIN_NAME}-${VERSION}-${ARCH}.zip"

    # Create staging directory
    mkdir -p "$STAGE_DIR"

    # PERFORMANCE: Copy only necessary files (exclude PowerToys/Wox dependencies and PDB files)
    find "$BUILD_PATH" -type f \
        ! -name "PowerToys*.dll" \
        ! -name "PowerToys*.pdb" \
        ! -name "Wox*.dll" \
        ! -name "Wox*.pdb" \
        ! -name "*.pdb" \
        -exec cp {} "$STAGE_DIR/" \;

    # Copy plugin.json and Images
    cp "$PLUGIN_DIR/plugin.json" "$STAGE_DIR/"
    if [ -d "$PLUGIN_DIR/Images" ]; then
        cp -r "$PLUGIN_DIR/Images" "$STAGE_DIR/"
    fi

    # PERFORMANCE: Create zip with optimal compression
    # -q: quiet mode for faster execution
    # -9: maximum compression
    (cd "$PUBLISH_DIR/$ARCH" && zip -q -9 -r "$ZIP_PATH" "$PLUGIN_NAME")

    local SIZE=$(du -h "$ZIP_PATH" | cut -f1)
    echo "✅ Created $ARCH package: $(basename "$ZIP_PATH") ($SIZE)"
}

# ===== PARALLEL PACKAGING =====
echo "⚡ Packaging both platforms in parallel..."
package_build "win-x64" &
PID_PKG_X64=$!

package_build "win-arm64" &
PID_PKG_ARM64=$!

# Wait for both packaging jobs to complete
wait $PID_PKG_X64 || { echo "❌ x64 packaging failed"; exit 1; }
wait $PID_PKG_ARM64 || { echo "❌ ARM64 packaging failed"; exit 1; }

echo ""
echo "✅ All packages created successfully!"
echo ""

# ===== PARALLEL CHECKSUM GENERATION =====
echo "🔐 Generating checksums..."
mkdir -p "$ROOT_DIR/checksums"

generate_checksum() {
    local FILE=$1
    local CHECKSUM=$(sha256sum "$FILE" | awk '{print toupper($1)}')
    local BASENAME=$(basename "$FILE")
    echo "$CHECKSUM  $BASENAME" > "${FILE}.sha256"
    echo "  ✓ $BASENAME: $CHECKSUM"
}

# Generate checksums in parallel
for file in "${PLUGIN_NAME}-${VERSION}-"*.zip; do
    generate_checksum "$file" &
done

# Wait for all checksum jobs
wait

# Create combined checksums file
{
    echo "SHA256 Checksums for $PLUGIN_NAME Plugin v$VERSION"
    echo "Generated: $(date -u '+%Y-%m-%d %H:%M:%S UTC')"
    echo ""
    cat "${PLUGIN_NAME}-${VERSION}-"*.zip.sha256
} > "checksums.txt"

echo ""
echo "🎉 Done! Build artifacts:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
ls -lh "${PLUGIN_NAME}-${VERSION}-"*.zip
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "📋 Checksums saved to: checksums.txt"
