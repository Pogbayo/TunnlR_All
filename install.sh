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

# Config directory for appsettings.json
CONFIG_DIR="$HOME/.tunnlr"

# Download the binary with error handling
echo "Downloading $BINARY..."
if curl -L "$DOWNLOAD_URL" -o tunnlr --fail --silent --show-error; then
    echo "✓ Download complete!"
else
    echo ""
    echo "✗ Installation failed!"
    echo "Error: Unable to download from GitHub"
    echo ""
    echo "Please download manually from:"
    echo "https://github.com/Pogbayo/TunnlR_All/releases/latest"
    echo ""
    echo "After downloading:"
    echo "1. chmod +x $BINARY"
    echo "2. sudo mv $BINARY /usr/local/bin/tunnlr"
    exit 1
fi

# Make the binary executable (required on Unix systems)
echo "Installing to $INSTALL_DIR..."
chmod +x tunnlr

# Move to system directory (requires sudo/admin password)
if sudo mv tunnlr "$INSTALL_PATH"; then
    echo "✓ Binary installed!"
else
    echo ""
    echo "✗ Installation failed!"
    echo "Could not move binary to $INSTALL_DIR"
    echo "You may need administrator privileges"
    exit 1
fi

# Create config directory and appsettings.json
echo "Creating configuration file..."
mkdir -p "$CONFIG_DIR"

cat > "$CONFIG_DIR/appsettings.json" << 'EOF'
{
  "RelayServer": {
    "HttpUrl": "https://tech-expert-beta.com.ng",
    "WebSocketUrl": "wss://tech-expert-beta.com.ng/tunnel"
  },
  "LocalServer": {
    "BaseUrl": "http://localhost",
    "DefaultPort": 5000
  }
}
EOF

echo "✓ Configuration created!"

echo ""
echo "✓ TunnlR installed successfully!"
echo "Run: tunnlr"