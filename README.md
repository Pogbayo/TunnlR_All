# TunnlR 🚇

> A lightweight ngrok alternative that exposes your localhost to the internet securely using WebSockets and JWT authentication.

TunnlR allows developers to create secure tunnels to their local development servers, making them accessible via public URLs. Built with .NET 9 and clean architecture principles.

## ✨ What Makes TunnlR Different?

- **Persistent URLs** - Your tunnel URL stays the same across reconnections
- **Secure by Default** - JWT authentication for all connections
- **Real-time** - WebSocket-based tunnel connections
- **Self-hostable** - Deploy on your own AWS EC2 instance
- **Production Ready** - Full SSL/TLS support with automatic certificate management
- **Clean Architecture** - Maintainable, testable codebase
- **Easy Installation** - One-command installation for all platforms

---

## 🎯 Current Features (MVP)

### ✅ Authentication System
- User registration with email/password
- JWT token-based authentication
- Secure token storage on developer's machine
- Token expiration handling (60-hour validity)

### ✅ Tunnel Management
- One persistent tunnel per user
- Automatic tunnel reuse on reconnection
- Unique subdomain generation (e.g., `9816bd07.tech-expert-beta.com.ng`)
- Tunnel status tracking (Active/Inactive)
- Wildcard SSL certificate support for dynamic subdomains

### ✅ CLI Tool
- Simple command-line interface for developers
- Interactive login/registration flow
- Tunnel start/stop commands
- Real-time connection status updates
- Animated console output with color-coded messages
- Cross-platform support (Windows, macOS, Linux)
- One-command installation

### ✅ WebSocket Infrastructure
- Persistent WebSocket connections between CLI and Proxy server
- Connection lifecycle management
- Automatic reconnection handling
- Multiple concurrent tunnel support
- Secure WebSocket (WSS) support in production

### ✅ Proxy Server (API)
- RESTful authentication endpoints
- WebSocket tunnel endpoint
- Connection pooling and management
- JWT token validation
- SQL Server database for data persistence
- Systemd service for production deployment
- Nginx reverse proxy integration

### ✅ Production Deployment
- AWS EC2 deployment support
- Nginx reverse proxy configuration
- Let's Encrypt SSL/TLS certificates
- Wildcard certificate support
- Systemd service management
- Cloudflare CDN integration
- Auto-restart on failure
- Certificate auto-renewal
- CI/CD pipeline with GitHub Actions

---

## 🏗️ Architecture

TunnlR follows Clean Architecture principles with clear separation of concerns:
```
TunnlR/
├── TunnlR.Domain          # Entities, DTOs, Enums
├── TunnlR.Application     # Business Logic, Services, Interfaces
├── TunnlR.Infrastructure  # Database, Identity, External Services
├── TunnlR.API            # Proxy Server (ASP.NET Core WebAPI)
├── TunnlR.CLI            # Command-Line Tool (Console App)
└── TunnlR.Contract       # Shared DTOs and Interfaces between API and CLI
```

### Technology Stack

**Backend (Proxy Server):**
- .NET 9.0
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server Database
- ASP.NET Core Identity
- JWT Authentication
- WebSockets

**CLI Tool:**
- .NET 9.0 Console Application
- System.Net.WebSockets
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection

**Infrastructure:**
- AWS EC2 (Ubuntu 22.04 LTS) - t3.small or larger recommended
- Nginx (Reverse Proxy)
- Let's Encrypt (SSL Certificates)
- Cloudflare (DNS & CDN)
- Systemd (Service Management)
- GitHub Actions (CI/CD)

---

## 📋 Prerequisites

### For Users (Installing CLI)
- No prerequisites needed for standalone binaries
- OR .NET 9.0 Runtime (for .NET global tool installation)

### For Developers (Building from source)
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Git (for cloning the repository)
- A code editor (Visual Studio 2022, VS Code, or Rider)

---

## 🚀 Quick Start (For Users)

### Installation

TunnlR CLI can be installed with a single command on any platform.

#### Windows

Open PowerShell and run:
```powershell
iwr https://raw.githubusercontent.com/Pogbayo/TunnlR_All/master/install.ps1 -UseBasicParsing | iex
```

If the download is slow or fails:
```powershell
iwr https://raw.githubusercontent.com/Pogbayo/TunnlR_All/master/install.ps1 -OutFile install.ps1 -UseBasicParsing
.\install.ps1
```

#### macOS / Linux

Open Terminal and run:
```bash
curl -sSL https://raw.githubusercontent.com/Pogbayo/TunnlR_All/master/install.sh | bash
```

#### Manual Installation

Download the appropriate binary for your platform from the [Releases page](https://github.com/Pogbayo/TunnlR_All/releases/latest):

- **Windows:** `tunnlr-windows-x64.exe`
- **macOS (Intel):** `tunnlr-macos-intel`
- **macOS (Apple Silicon):** `tunnlr-macos-arm64`
- **Linux:** `tunnlr-linux-x64`

**Windows:**
```powershell
# Create installation directory
mkdir C:\tunnlr

# Move downloaded file
move Downloads\tunnlr-windows-x64.exe C:\tunnlr\tunnlr.exe

#Create an AppSettins file(appsettings.json) in the same directory with the following content
{
   "ProxyServer": {
    "HttpUrl": "https://tech-expert-beta.com.ng",
    "WebSocketUrl": "wss://tech-expert-beta.com.ng/tunnel"
  },
  "LocalServer": {
    "BaseUrl": "http://localhost",
    "DefaultPort": 5000
  }
}

# Add to PATH
setx PATH "%PATH%;C:\tunnlr"

# Restart terminal and run
tunnlr
```

**macOS/Linux:**
```bash
# Make executable
chmod +x tunnlr-macos-intel

# Move to system directory
sudo mv tunnlr-macos-intel /usr/local/bin/tunnlr

#Create an AppSettins file(appsettings.json) in the same directory with the following content
{
   "ProxyServer": {
    "HttpUrl": "https://tech-expert-beta.com.ng",
    "WebSocketUrl": "wss://tech-expert-beta.com.ng/tunnel"
  },
  "LocalServer": {
    "BaseUrl": "http://localhost",
    "DefaultPort": 5000
  }
}

# Run from anywhere
tunnlr
```

### First Time Usage

After installation, open a new terminal and run:
```bash
tunnlr
```

You'll see the TunnlR CLI interface:
```
🚀 TunnlR CLI
━━━ Commands ━━━
  start --port 5000 --protocol http
    Start a new tunnel
  exit
    Exit the CLI
>
```

#### Register an Account
```
> register
Email: your-email@example.com
Password: ********
Confirm Password: ********

✅ Registration successful! You can now login.
```

#### Login
```
> login
Email: your-email@example.com
Password: ********

✅ Login successful!
Email: your-email@example.com
```

Your authentication token is securely saved locally at:
- **Windows:** `%APPDATA%\.tunnlr\token.txt`
- **macOS/Linux:** `~/.tunnlr/token.txt`

The token is valid for 60 hours. After expiration, you'll need to login again.

#### Start a Tunnel

Ensure you have a local web server running (e.g., on port 5000), then:
```
> start --port 5000

Starting tunnel on port 5000...
Connecting ⠋

✓ Tunnel started

┌─────────────────────────────────────────────────
│ Public URL  https://9816bd07.tech-expert-beta.com.ng
│ Dashboard   https://dashboard.tunnlr.dev/9816bd07
└─────────────────────────────────────────────────

Press Ctrl+C to stop the tunnel...
```

Your local server on port 5000 or whichever port you specify is now accessible via the public URL!

#### Stop the Tunnel

Press `Ctrl+C` in the terminal where the tunnel is running, or:
```
> stop
```

#### Exit the CLI
```
> exit
```

---

## 🔧 Building from Source

### 1. Clone the Repository
```bash
git clone https://github.com/Pogbayo/TunnlR_All.git
cd TunnlR_All
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Configure the Proxy Server

Update `TunnlR.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TunnlRDb;User Id=tunnlr_user;Password=your_password;Encrypt=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "your-super-secret-key-min-32-characters-long",
    "Issuer": "TunnlR",
    "Audience": "TunnlR_CLI",
    "ExpiryHours": 60
  }, 
  "ServerUrl": {
     "Host": "your-domain.com"
  }
}
```

### 4. Apply Database Migrations
```bash
cd TunnlR.API
dotnet ef database update
```

### 5. Run the Proxy Server
```bash
cd TunnlR.API
dotnet run
```

The API will start on `http://localhost:5248`

### 6. Configure the CLI Tool

Update `TunnlR.CLI/appsettings.json`:
```json
{
  "ProxyServer": {
    "HttpUrl": "https://your-Proxy-server.com",
    "WebSocketUrl": "wss://your-Proxy-server.com/tunnel"
  },
  "LocalServer": {
    "BaseUrl": "http://localhost",
    "DefaultPort": 5000
  }
}
```

### 7. Build the CLI Tool
```bash
cd TunnlR.CLI
dotnet build
dotnet run
```

---

## 🌍 Production Deployment (AWS EC2)

### Server Setup

TunnlR is deployed on AWS EC2 with Nginx as a reverse proxy and Let's Encrypt for SSL/TLS.

#### Prerequisites

- AWS EC2 instance (t3.small or larger recommended)
- Ubuntu 22.04 LTS
- Domain name configured with Cloudflare DNS
- Security group with ports 80 and 443 open
- .NET 9.0 Runtime

**Note on Instance Size:** Use at minimum a t3.small instance (2GB RAM). The t3.micro (1GB RAM) is insufficient and will cause out-of-memory errors.

#### 1. Install Required Dependencies
```bash
# Update system packages
sudo apt update && sudo apt upgrade -y

# Install .NET 9.0 Runtime
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0

# Install Nginx
sudo apt install nginx -y

# Install Certbot for SSL certificates
sudo apt install certbot python3-certbot-nginx python3-certbot-dns-cloudflare -y

# Install SQL Server (if using SQL Server)
# Follow Microsoft's official guide for SQL Server on Linux
```

#### 2. Deploy the Application
```bash
# Clone the repository
git clone https://github.com/Pogbayo/TunnlR_All.git /var/www/TunnlR_All

# Navigate to API folder
cd /var/www/TunnlR_All/TunnlR.API

# Publish the application
dotnet publish -c Release -o publish
```

#### 3. Create Systemd Service

Create `/etc/systemd/system/tunnlr-api.service`:
```ini
[Unit]
Description=TunnlR ASP.NET API
After=network.target

[Service]
WorkingDirectory=/var/www/TunnlR_All/TunnlR.API/publish
ExecStart=/usr/bin/dotnet /var/www/TunnlR_All/TunnlR.API/publish/TunnlR.API.dll
Restart=always
RestartSec=10
SyslogIdentifier=tunnlr-api
User=ubuntu
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_ROOT=/usr/share/dotnet
Environment=HOME=/home/ubuntu

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl daemon-reload
sudo systemctl enable tunnlr-api.service
sudo systemctl start tunnlr-api.service
sudo systemctl status tunnlr-api.service
```

#### 4. Configure Nginx

Create `/etc/nginx/sites-available/tunnlr`:
```nginx
# HTTP to HTTPS redirect
server {
    listen 80;
    server_name tech-expert-beta.com.ng *.tech-expert-beta.com.ng;
    return 301 https://$host$request_uri;
}

# HTTPS with SSL and WebSocket support
server {
    listen 443 ssl;
    server_name tech-expert-beta.com.ng *.tech-expert-beta.com.ng;

    ssl_certificate /etc/letsencrypt/live/tech-expert-beta.com.ng/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/tech-expert-beta.com.ng/privkey.pem;

    location / {
        proxy_pass http://localhost:5248;
        proxy_http_version 1.1;
        
        # WebSocket support
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        
        # Pass client information
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable site:
```bash
sudo ln -s /etc/nginx/sites-available/tunnlr /etc/nginx/sites-enabled/
sudo rm /etc/nginx/sites-enabled/default
sudo nginx -t
sudo systemctl restart nginx
```

#### 5. Obtain Wildcard SSL Certificates

Create Cloudflare credentials file `/etc/letsencrypt/cloudflare.ini`:
```ini
dns_cloudflare_api_token = your_cloudflare_api_token
```

Secure it:
```bash
sudo chmod 600 /etc/letsencrypt/cloudflare.ini
```

Request certificate:
```bash
sudo certbot certonly --dns-cloudflare \
  --dns-cloudflare-credentials /etc/letsencrypt/cloudflare.ini \
  -d tech-expert-beta.com.ng \
  -d *.tech-expert-beta.com.ng
```

Certificates auto-renew every 60 days via cron.

#### 6. Configure Cloudflare

In Cloudflare Dashboard:
1. Add A record pointing to EC2 public IP
2. Enable proxy (orange cloud) for DDoS protection
3. Set SSL/TLS mode to "Full"

#### 7. Update Production Configuration

Update `/var/www/TunnlR_All/TunnlR.API/appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TunnlRDb;User Id=tunnlr_user;Password=your_password;Encrypt=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "your-production-secret-key-minimum-32-characters",
    "Issuer": "TunnlR",
    "Audience": "TunnlR_CLI",
    "ExpiryHours": 60
  },
  "ServerUrl": {
    "Host": "tech-expert-beta.com.ng"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5248"
      }
    }
  }
}
```

Rebuild and restart:
```bash
cd /var/www/TunnlR_All/TunnlR.API
dotnet publish -c Release -o publish
sudo systemctl restart tunnlr-api.service
```

### CI/CD Pipeline

The project includes GitHub Actions workflow for automated deployment:

**`.github/workflows/tunnlr-ci.yml`:**
```yaml
name: TunnlR CI
on:
  push:
    branches:
      - master
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 9.0.x
      
      - name: Restore
        run: dotnet restore
      
      - name: Build
        run: dotnet build --configuration Release
        
  deploy: 
    needs: build
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Deploy to EC2
        uses: appleboy/ssh-action@v1
        with:
          host: ${{ secrets.EC2_HOST }}
          username: ${{ secrets.EC2_USER }}
          key: ${{ secrets.EC2_SSH_KEY }}
          script: |
             cd /var/www/TunnlR_All
            git pull origin master
            dotnet build -c Release -o out
            sudo systemctl restart tunnlr
```

Configure GitHub Secrets:
- `EC2_HOST`: Your EC2 public IP
- `EC2_USER`: SSH username (ubuntu)
- `EC2_SSH_KEY`: Private SSH key

### Traffic Flow in Production
```
Internet User (HTTPS) ✅ Encrypted
    ↓
Cloudflare (DDoS protection, CDN) ✅ Encrypted
    ↓
AWS EC2 - Nginx (Port 443, SSL termination) ✅ Encrypted
    ↓
.NET Application (localhost:5248, HTTP) ❌ Unencrypted (safe - localhost only)
    ↓
WebSocket to CLI (WSS) ✅ Encrypted
    ↓
CLI on Developer's Machine
    ↓
Local App (localhost:5000, HTTP) ❌ Unencrypted (safe - localhost only)
```

**Security Note:** Unencrypted localhost traffic is safe as it never leaves the machine. All internet traffic is fully encrypted end-to-end.

### Monitoring and Maintenance
```bash
# View application logs in real-time
sudo journalctl -u tunnlr-api.service -f

# View Nginx logs
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log

# Restart services
sudo systemctl restart tunnlr-api.service
sudo systemctl restart nginx

# Check SSL certificate status
sudo certbot certificates

# Test certificate renewal
sudo certbot renew --dry-run
```

### Troubleshooting

**Service won't start:**
```bash
sudo systemctl status tunnlr-api.service
sudo journalctl -u tunnlr-api.service -n 50 --no-pager
```

**Nginx errors:**
```bash
sudo nginx -t
sudo nginx -s reload
```

**Database connection issues:**
```bash
# Test SQL Server connection
sqlcmd -S localhost -U tunnlr_user -P your_password

# Grant database creation permissions (if needed)
sudo visudo
# Add: ubuntu ALL=(ALL) NOPASSWD: /bin/systemctl restart tunnlr-api.service
```

**WebSocket connection failures:**
Check that:
- Nginx has WebSocket upgrade headers configured
- Cloudflare proxy is enabled
- Firewall allows ports 80/443

---

## 🔐 Security

### Encryption

- **Internet Traffic:** Fully encrypted with HTTPS/WSS
- **Localhost Traffic:** Unencrypted but isolated (cannot be intercepted)
- **WebSocket Connections:** Secured with WSS protocol
- **JWT Tokens:** 256-bit HMAC-SHA256 signing

### Authentication Flow

1. User registers with email/password
2. Password hashed using ASP.NET Core Identity (PBKDF2)
3. JWT token generated on login (60-hour expiration)
4. Token stored locally on user's machine
5. All tunnel connections require valid JWT
6. Token sent as query parameter in WebSocket handshake

### JWT Token Structure
```json
{
  "nameid": "user-guid",
  "email": "user@example.com",
  "jti": "token-guid",
  "exp": 1771234567,
  "iss": "TunnlR",
  "aud": "TunnlR_CLI"
}
```

### Best Practices

- Always use strong JWT secret keys (minimum 32 characters)
- HTTPS/WSS required in production
- Tokens expire after 60 hours
- Passwords never stored in plain text
- Database credentials stored in environment variables or secrets management

---

## 🔧 Configuration

### Proxy Server Configuration

**File:** `TunnlR.API/appsettings.json`
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters-long",
    "Issuer": "TunnlR",
    "Audience": "TunnlR_CLI",
    "ExpiryHours": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TunnlRDb;User Id=tunnlr_user;Password=your_password;Encrypt=True;TrustServerCertificate=True;"
  },
  "ServerUrl": {
    "Host": "tech-expert-beta.com.ng"
  },
  "AllowedHosts": "*"
}
```

**Configuration Options:**

| Key | Description | Example |
|-----|-------------|---------|
| `ConnectionStrings:DefaultConnection` | Database connection string | SQL Server connection |
| `Jwt:Key` | Secret key for JWT signing | Minimum 32 characters |
| `Jwt:Issuer` | JWT token issuer | `TunnlR` |
| `Jwt:Audience` | JWT token audience | `TunnlR_CLI` |
| `Jwt:ExpiryHours` | Token validity period | `60` (hours) |
| `ServerUrl:Host` | Public domain name | `your-domain.com` |

### CLI Configuration

**File:** `TunnlR.CLI/appsettings.json`
```json
{
  "ProxyServer": {
    "HttpUrl": "https://tech-expert-beta.com.ng",
    "WebSocketUrl": "wss://tech-expert-beta.com.ng/tunnel"
  },
  "LocalServer": {
    "BaseUrl": "http://localhost",
    "DefaultPort": 5000
  }
}
```

**Configuration Options:**

| Key | Description | Example |
|-----|-------------|---------|
| `ProxyServer:HttpUrl` | Proxy server HTTP endpoint | `https://your-server.com` |
| `ProxyServer:WebSocketUrl` | WebSocket endpoint | `wss://your-server.com/tunnel` |
| `LocalServer:BaseUrl` | Local server protocol | `http://localhost` |
| `LocalServer:DefaultPort` | Default port if not specified | `5000` |

**Token Storage:**

Authentication tokens are securely stored at:
- **Windows:** `%LOCALAPPDATA%\tunnlr\token.txt`
- **macOS/Linux:** `~/.tunnlr/token.txt`

**CLI Binary Location:**

After installation:
- **Windows:** `%LOCALAPPDATA%\tunnlr\tunnlr.exe`
- **macOS/Linux:** `/usr/local/bin/tunnlr`

---

## 🌐 API Endpoints

### Authentication

#### Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!"
}

Response 200 OK:
{
  "success": true,
  "message": "Registration successful"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}

Response 200 OK:
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "email": "user@example.com",
  "expiresAt": "2026-02-18T12:00:00Z"
}
```

### Tunnels

#### Get Tunnel Status
```http
GET /api/tunnel/{tunnelId}/status
Authorization: Bearer {token}

Response 200 OK:
{
  "tunnelId": "guid",
  "isActive": true,
  "bytesTransferred": 0,
  "requestCount": 0
}
```

#### Deactivate Tunnel
```http
DELETE /api/tunnel/deactivate/{tunnelId}
Authorization: Bearer {token}

Response 204 No Content
```

### WebSocket

#### Tunnel Connection
```
wss://your-server.com/tunnel?token={jwt}&port={localPort}&protocol={http}

Server → CLI (on successful connection):
{
  "tunnelId": "guid",
  "publicUrl": "https://9816bd07.tech-expert-beta.com.ng",
  "dashboardUrl": "https://dashboard.tunnlr.dev/9816bd07"
}

Server → CLI (incoming request):
{
  "requestId": "guid",
  "method": "GET",
  "path": "/api/users",
  "headers": { ... },
  "body": ""
}

CLI → Server (response):
{
  "requestId": "guid",
  "statusCode": 200,
  "body": "...",
  "headers": { ... }
}
```

---

## 🚧 Known Limitations (Current MVP)

- ⚠️ **Single tunnel per user** - Multiple tunnels support planned
- ⚠️ **No web dashboard** - Dashboard URL is currently a placeholder
- ⚠️ **No request history** - Traffic logging coming in next phase
- ⚠️ **HTTP only** - HTTPS local servers not yet supported

---

## 🗺️ Roadmap

### Phase 1: Core Functionality ✅ COMPLETE
- [x] User authentication
- [x] Tunnel creation and management
- [x] WebSocket connections
- [x] CLI tool with cross-platform support
- [x] Production deployment infrastructure
- [x] SSL/TLS with wildcard certificates
- [x] HTTP traffic forwarding
- [x] One-command installation
- [x] CI/CD pipeline

### Phase 2: Traffic Management (In Progress)
- [ ] Request/response logging
- [ ] Traffic inspection dashboard
- [ ] Request replay functionality
- [ ] Custom response modification

### Phase 3: Dashboard & Monitoring (Planned)
- [ ] Web-based dashboard
- [ ] Real-time traffic visualization
- [ ] Analytics and insights
- [ ] Webhook support
- [ ] API rate limiting
- [ ] Usage statistics

### Phase 4: Advanced Features (Coming Soon)
- [ ] Multiple tunnels per user
- [ ] Custom subdomain names
- [ ] HTTPS support for local servers
- [ ] Tunnel sharing (public/private)
- [ ] Team collaboration features
- [ ] Bandwidth usage tracking
- [ ] Custom domains

---

## 🤝 Contributing (Not yet available)

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

- GitHub: [@Pogbayo](https://github.com/Pogbayo)
- Email: adebayooluwasegun335@gmail.com

---

## 🙏 Acknowledgments

- Inspired by [ngrok](https://ngrok.com/)
- Built with [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- Clean Architecture principles by [Jason Taylor](https://github.com/jasontaylordev/CleanArchitecture)

---

**Made with ❤️ by developers, for developers**