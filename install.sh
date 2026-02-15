#!/bin/bash
# This is a bash script (shell scripting for Unix-based systems)
# The line above tells the system to use bash to run this script

# Exit immediately if any command fails (safety measure)
set -e

echo "Installing TunnlR CLI..."

# Detect the operating system
# uname -s returns "Darwin" (macOS), "Linux", etc.
# tr converts uppercase to lowercase for easier comparison
OS=$(uname -s | tr '[:upper:]' '[:lower:]')

# Detect CPU architecture
# uname -m returns "arm64" (Apple Silicon), "x86_64" (Intel), etc.
ARCH=$(uname -m)

# Determine which binary to download based on OS and architecture
case "$OS" in
    darwin)  # macOS
        if [ "$ARCH" = "arm64" ]; then
            # Apple Silicon Macs (M1, M2, M3, etc.)
            BINARY="tunnlr-macos-arm64"
        else
            # Intel Macs
            BINARY="tunnlr-macos-intel"
        fi
        ;;
    linux)  # Linux
        # Most Linux systems are x86_64
        BINARY="tunnlr-linux-x64"
        ;;
    *)  # Unknown OS
        echo "Unsupported OS: $OS"
        exit 1  # Exit with error code
        ;;
esac

# Build the download URL
DOWNLOAD_URL="https://github.com/Pogbayo/TunnlR_All/releases/latest/download/$BINARY"

# Installation directory (standard location for user-installed binaries)
# /usr/local/bin is in PATH by default on macOS/Linux
INSTALL_DIR="/usr/local/bin"

# Full path where tunnlr will be installed
INSTALL_PATH="$INSTALL_DIR/tunnlr"

# Download the binary
echo "Downloading $BINARY..."
curl -L "$DOWNLOAD_URL" -o tunnlr  # -L follows redirects, -o saves to file

# Make the binary executable (required on Unix systems)
echo "Installing to $INSTALL_DIR..."
chmod +x tunnlr  # chmod +x adds execute permission

# Move to system directory (requires sudo/admin password)
sudo mv tunnlr "$INSTALL_PATH"

# Success message
echo ""
echo "✓ TunnlR installed successfully!"
echo "Run: tunnlr"