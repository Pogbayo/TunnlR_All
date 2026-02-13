TunnlR 🚇

> A lightweight ngrok alternative that exposes your localhost to the internet securely using WebSockets and JWT authentication.

TunnlR allows developers to create secure tunnels to their local development servers, making them accessible via public URLs. Built with .NET 9 and clean architecture principles.

## ✨ What Makes TunnlR Different?

- **Persistent URLs** - Your tunnel URL stays the same across reconnections
- **Secure by Default** - JWT authentication for all connections
- **Real-time** - WebSocket-based tunnel connections
- **Self-hostable** - Deploy on your own AWS EC2 instance
- **Production Ready** - Full SSL/TLS support with automatic certificate management
- **Clean Architecture** - Maintainable, testable codebase

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
- Unique subdomain generation (e.g,`abc12345.tunnlr.dev`) N:B This is a placeholder and will be replaced with the actual domain in production
- Tunnel status tracking (Active/Inactive)
- Wildcard SSL certificate support for dynamic subdomains

### ✅ CLI Tool
- Simple command-line interface for developers
- Interactive login/registration flow
- Tunnel start/stop commands
- Real-time connection status updates
- Animated console output with color-coded messages

### ✅ WebSocket Infrastructure
- Persistent WebSocket connections between CLI and relay server
- Connection lifecycle management
- Automatic reconnection handling
- Multiple concurrent tunnel support
- Secure WebSocket (WSS) support in production

### ✅ Relay Server (API)
- RESTful authentication endpoints
- WebSocket tunnel endpoint
- Connection pooling and management
- JWT token validation
- SQLite database for data persistence
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

---

## 🏗️ Architecture

TunnlR follows Clean Architecture principles with clear separation of concerns:
```
TunnlR/
├── TunnlR.Domain          # Entities, DTOs, Enums
├── TunnlR.Application     # Business Logic, Services, Interfaces
├── TunnlR.Infrastructure  # Database, Identity, External Services
├── TunnlR.API            # Relay Server (ASP.NET Core WebAPI)
└── TunnlR.CLI            # Command-Line Tool (Console App)
└── TunnlR.Contracts       # Shared DTOs and Interfaces between API and CLI
```

### Technology Stack

**Backend (Proxy Server):**
- .NET 9.0
- ASP.NET Core Web API
- Entity Framework Core
- SQLite Database
- ASP.NET Core Identity
- JWT Authentication
- WebSockets

**CLI Tool:**
- .NET 9.0 Console Application
- System.Net.WebSockets
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection

**Infrastructure:**
- AWS EC2 (Ubuntu 22.04 LTS)
- Nginx (Reverse Proxy)
- Let's Encrypt (SSL Certificates)
- Cloudflare (DNS & CDN)
- Systemd (Service Management)

---

## 📋 Prerequisites

Before running TunnlR, ensure you have:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Git (for cloning the repository)
- A code editor (Visual Studio 2022, VS Code, or Rider)

---

## 🚀 Quick Start

### 1. Clone the Repository
```bash
git clone https://github.com/Pogbayo/tunnlr.git
cd tunnlr
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Configure the Relay Server

Update `TunnlR.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tunnlr.db"
  },
  "Jwt": {
    "Key": "your-super-secret-key-min-32-characters-long",
    "Issuer": "TunnlR",
    "Audience": "TunnlR-CLI"
  }, 
  "ServerUrl": {
     "BaseUrl": "https://domain.com" 
 },
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

The API will start on `https://localhost:5248` (or the port configured in `launchSettings.json`)

### 6. Configure the CLI Tool

Update `TunnlR.CLI/appsettings.json`:
```json
{
  "RelayServer": {
    "HttpUrl": "https://relayserver:5248",
    "WebSocketUrl": "wss://websocket:5248"
  }
}
```

### 7. Build the CLI Tool
```bash
cd TunnlR.CLI
dotnet build
```

---

## 🌍 Production Deployment (AWS EC2)

### Server Setup

TunnlR can be deployed on AWS EC2 for production use. This setup uses Nginx as a reverse proxy with SSL/TLS encryption.

#### Prerequisites

- AWS EC2 instance (Ubuntu 22.04 LTS recommended)
- Make sure to use a minimum of t3.small becuase it is the least capacity instance that can run the application without crashing due to insufficient memory. The t3.micro instance has only 1GB of RAM which is not enough to run the application and it will crash due to out of memory error. The t3.small instance has 2GB of RAM which is sufficient to run the application smoothly.
- Domain name configured with Cloudflare DNS
- Port 80 and 443 open in security group
- .NET 9.0 Runtime installed on the server


After SSHing into your EC2 instance, follow these steps to deploy TunnlR:

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
```

#### 2. Deploy the Application

```bash
# Clone the repository
git clone https://github.com/Pogbayo/tunnlr.git
cd tunnlr/TunnlR.API

# Publish the application
dotnet publish -c Release -o /var/www/TunnlR_All/TunnlR.API/publish
```

#### 3. Create Systemd Service

Create a systemd service file to run TunnlR as a background service:

```bash
sudo nano /etc/systemd/system/tunnlr-api.service
```

Add the following configuration:

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

Enable and start the service:

```bash
# Reload systemd daemon
sudo systemctl daemon-reload

# Enable service to start on boot
sudo systemctl enable tunnlr-api.service

# Start the service
sudo systemctl start tunnlr-api.service

# Check service status
sudo systemctl status tunnlr-api.service
```

#### 4. Configure Nginx Reverse Proxy

Create Nginx configuration file:

```bash
sudo nano /etc/nginx/sites-available/tunnlr
```

Add the following configuration:

```nginx
# Redirect HTTP to HTTPS
server {
    listen 80;
    server_name your-domain.com *.your-domain.com;
    return 301 https://$host$request_uri;
}

# HTTPS server with SSL and WebSocket support
server {
    listen 443 ssl;
    server_name your-domain.com *.your-domain.com;

    # SSL Certificate paths (will be configured by Certbot)
    ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;

    location / {
        # Forward to .NET app
        proxy_pass http://localhost:5248;
        proxy_http_version 1.1;
        
        # WebSocket support
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        
        # Pass client information to backend
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable the site and remove default configuration:

```bash
# Enable the site
sudo ln -s /etc/nginx/sites-available/tunnlr /etc/nginx/sites-enabled/

# Remove default configuration
sudo rm /etc/nginx/sites-enabled/default

# Test configuration
sudo nginx -t

# Restart Nginx
sudo systemctl restart nginx
```

#### 5. Obtain SSL Certificates

TunnlR requires wildcard SSL certificates to support dynamic subdomains for each tunnel.

##### Configure Cloudflare API Token

1. Go to Cloudflare Dashboard → My Profile → API Tokens
2. Create token with "Edit zone DNS" permissions
3. Select "All zones" under Zone Resources
4. Copy the generated token

Create credentials file:

```bash
sudo nano /etc/letsencrypt/cloudflare.ini
```

Add your Cloudflare API token:

```ini
dns_cloudflare_api_token = your_cloudflare_api_token_here
```

Secure the credentials file:

```bash
sudo chmod 600 /etc/letsencrypt/cloudflare.ini
```

##### Request Wildcard Certificate

```bash
sudo certbot certonly --dns-cloudflare \
  --dns-cloudflare-credentials /etc/letsencrypt/cloudflare.ini \
  -d your-domain.com \
  -d *.your-domain.com
```

When prompted, choose to expand and replace existing certificates.

#### 6. Configure Cloudflare DNS

In your Cloudflare dashboard:

1. Add an A record pointing to your EC2 instance's public IP
2. Enable proxy (orange cloud) for DDoS protection
3. Set SSL/TLS mode to "Full" under SSL/TLS → Overview

#### 7. Update Production Configuration

Update `TunnlR.API/appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/var/www/TunnlR_All/TunnlR.API/tunnlr.db"
  },
  "Jwt": {
    "Key": "your-production-secret-key-min-32-characters",
    "Issuer": "TunnlR",
    "Audience": "TunnlR-CLI"
  },
   "ServerUrl": {
     "BaseUrl": "https://your_doamin.com_"
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
dotnet publish -c Release -o publish --no-incremental --force
sudo systemctl restart tunnlr-api.service
```

### Traffic Flow in Production

```
User Request (HTTPS)
    ↓
Cloudflare (DDoS protection, CDN)
    ↓
AWS EC2 - Nginx (Port 443, SSL termination)
    ↓
.NET Application (Port 5248, HTTP)
    ↓
Response back through same path
```

### Key Features of Production Setup

- **HTTPS/SSL:** All traffic encrypted with Let's Encrypt certificates
- **Wildcard Certificates:** Supports dynamic subdomains (e.g., `abc123.your-domain.com`)
- **WebSocket Support:** Nginx configured for WebSocket connections (WSS)
- **Auto-restart:** Systemd service automatically restarts on failure
- **DDoS Protection:** Cloudflare proxy protects against attacks
- **Auto-renewal:** SSL certificates renew automatically every 60 days

### Monitoring and Maintenance

```bash
# View application logs
sudo journalctl -u tunnlr-api.service -f

# View Nginx logs
sudo tail -f /var/log/nginx/error.log
sudo tail -f /var/log/nginx/access.log

# Restart services
sudo systemctl restart tunnlr-api.service
sudo systemctl restart nginx

# Check SSL certificate expiry
sudo certbot certificates

# Test SSL renewal (dry run)
sudo certbot renew --dry-run
```

### Troubleshooting

**Service won't start:**
```bash
# Check service status
sudo systemctl status tunnlr-api.service

# View detailed logs
sudo journalctl -u tunnlr-api.service -n 50 --no-pager
```

**Nginx configuration errors:**
```bash
# Test Nginx configuration
sudo nginx -t

# Reload Nginx without downtime
sudo nginx -s reload
```

**SSL certificate issues:**
```bash
# Verify DNS is pointing to your server
nslookup your-domain.com

# Check certificate paths in Nginx config
ls -l /etc/letsencrypt/live/your-domain.com/
```

**Multiple service instances running:**
```bash
# Check for duplicate services
sudo systemctl list-units | grep -i tunnl

# Stop and disable duplicate services
sudo systemctl stop tunnlr.service
sudo systemctl disable tunnlr.service
```

---

## 💻 Using TunnlR CLI

### First Time Setup

#### 1. Install the TunnlR CLI

**Option A: Download prebuilt binary (recommended, no .NET required)**  
Download the appropriate binary for your OS from the GitHub Releases page.

```bash
# Linux / macOS
chmod +x tunnlr
./tunnlr
```

**Option B: Install as a .NET global tool (for .NET users)**

```bash
dotnet tool install --global TunnlR.CLI
```

---

#### 2. Register an Account

```bash
tunnlr
```

```text
> register
Email: your-email@example.com
Password: ********
Confirm Password: ********

✅ Registration successful! You can now login.
```


#### 3. Login

```bash
tunnlr
```

```text
> login
Email: your-email@example.com
Password: ********

✅ Login successful!
Email: your-email@example.com
```

After a successful login, an authentication token is securely saved on the local machine.

On subsequent runs, when a developer starts a tunnel using:

```bash
tunnlr start <port>
```

the CLI automatically checks the saved token:
- If the token is still valid, the tunnel starts immediately.
- If the token is missing or expired, the user is prompted to log in again before the tunnel can start.(The token only lasts for 60 hours).

- 

### Starting a Tunnel

Assuming you have a local web server running on port 3000:
```bash
>tunnlr start 3000

Starting tunnel on port 3000...
Connecting [====================]

✅ Connected to proxy server!

✅ Tunnel started!
Public URL: https://abc12345.tunnlr.dev **note once again, this is a dummy domain and will be replaced with the actual domain in production**
Dashboard: https://dashboard.tunnlr.dev/abc12345

Tunnel is running. Type 'stop' to close or press Ctrl+C...
```

### Stopping a Tunnel
```bash
> stop

Stopping tunnel...
✅ Tunnel stopped successfully
```

### Getting Help
```bash
> help

Commands:
register : Create a new account
login : Login to your account
start <port> : Start a new tunnel
stop : Stop the current tunnel
help : Show available commands
exit : Exit the CLI
```

### Exiting the CLI
```bash
> exit
```

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
        "Key": "h'3489gfvhnoirhoi[sh'nvh8ihnifp98h4774hfh3r[7fh7p843gf78g3748gfo87g478fg7o8gf78g7fgcwo7cfg7fo7g387fg873gp7h3fp87gp7dfb2puidp7gfpiugp87gfpiu3gp87fvouwhc98qhacna[ouch9[8hnfh3[8griowheh8ibnfoihernvoehf89[vhoinbd[uofhudnbubu[fhuofiigig8iotj]]]]]]]'ohd8hidh89wh89h82fh3", //fake key lol
        "Issuer": "TunnlR",
        "Audience": "TunnlR_CLI",
        "ExpireHours": 60
    },
    "ConnectionStrings": {
        "DefaultConnection": "your connection key"
    },
     "ServerUrl": {
     "BaseUrl": "https://tdomain.com"
 },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tunnlr.db"
  }
}
```

**Configuration Options:**

| Key | Description | Default |
|-----|-------------|---------|
| `ConnectionStrings:DefaultConnection` | SQLite database path | `Data Source=tunnlr.db` |
| `Jwt:Key` | Secret key for JWT signing (min 32 chars) | *Required* |
| `Jwt:Issuer` | JWT token issuer | `TunnlR` |
| `Jwt:Audience` | JWT token audience | `TunnlR-CLI` |

### CLI Configuration

**File:** `TunnlR.CLI/appsettings.json`
```json
{
 "RelayServer": {
    "HttpUrl": "https://relayserver:5001",
    "WebSocketUrl": "wss://websocket:5001"
  }
}
```

**Configuration Options:**

| Key | Description | Example |
|-----|-------------|---------|
| `RelayServer:HttpUrl` | Relay server HTTP endpoint | `https://your-server.com` |
| `RelayServer:WebSocketUrl` | Relay server WebSocket endpoint | `wss://your-server.com` |

**Token Storage:**

Authentication tokens are stored locally at:
- **Windows:** `%APPDATA%\.tunnlr\token.txt`
- **macOS/Linux:** `~/.tunnlr/token.txt`

---

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
  "expiresAt": "2026-02-05T12:00:00Z"
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
DELETE /api/tunnel/{tunnelId}
Authorization: Bearer {token}

Response 204 No Content
```

### WebSocket

#### Tunnel Connection
```
wss://your-server.com/tunnel?token={jwt}&port={localPort}&protocol={http|https}

Server → CLI:
{
  "tunnelId": "guid",
  "publicUrl": "https://abc12345.tunnlr.dev",
  "dashboardUrl": "https://dashboard.tunnlr.dev/abc12345"
}
```

---

## 🔐 Security

### Authentication Flow

1. User registers with email/password
2. Password is hashed using ASP.NET Core Identity (PBKDF2)
3. On login, JWT token is generated with 24-hour expiration
4. Token is stored locally on user's machine
5. All tunnel connections require valid JWT token
6. Token is sent as query parameter in WebSocket connection

### JWT Token Structure
```json
{
  "nameid": "user-guid",
  "email": "user@example.com",
  "jti": "token-guid",
  "exp": 1738761600,
  "iss": "TunnlR",
  "aud": "TunnlR-CLI"
}
```

### Best Practices

- Change the default JWT secret key in production
- Use HTTPS/WSS in production
- Tokens expire after 24 hours
- Passwords are never stored in plain text
- WebSocket connections are authenticated

---

## 🚧 Known Limitations (Current MVP)

- ⚠️ **No traffic inspection/logging** - Request/response logging coming in Phase 2
- ⚠️ **Single tunnel per user** - Multiple tunnels support planned for future
- ⚠️ **No dashboard UI** - Dashboard URL is a placeholder

---

## 🗺️ Roadmap

### Phase 1: Core Functionality (Current)
- [x] User authentication
- [x] Tunnel creation
- [x] WebSocket connections
- [x] CLI tool
- [x] Production deployment infrastructure
- [x] SSL/TLS support
- [x] Wildcard certificate support
- [ ] HTTP traffic forwarding (In Progress)

### Phase 2: Traffic Management
- [ ] Complete request/response forwarding
- [ ] Traffic inspection and logging
- [ ] Request/response dashboard (In Progress)

### Phase 3: Dashboard & Monitoring
- [ ] Web-based dashboard
- [ ] Real-time traffic visualization
- [ ] Analytics and insights
- [ ] Webhook support
- [ ] API rate limiting

### Phase 4: Advanced Features(Coming soon)
- [ ] Multiple tunnels per user
- [ ] Custom subdomain names
- [ ] HTTPS support for local servers
- [ ] Tunnel sharing (public/private)
- [ ] Team collaboration features

---


### Code Style

This project follows standard C# coding conventions:
- PascalCase for public members
- camelCase for private fields with `_` prefix
- Async methods end with `Async`
- Interfaces start with `I`

---

## 🤝 Contributing (Not yet avaliable)

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Not yet available ocne again
---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

- GitHub: [@Pogbayo](https://github.com/Pogbayo)
- Email:  adebayooluwasegun335@gmail.com

---

## 🙏 Acknowledgments

- Inspired by [ngrok](https://ngrok.com/)
- Built with [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- Clean Architecture principles by [Jason Taylor](https://github.com/jasontaylordev/CleanArchitecture)

---

**Made with ❤️ by developers, for developers**
Adebayo