# TunnlR 🚇

> A lightweight ngrok alternative that exposes your localhost to the internet securely using WebSockets and JWT authentication.

TunnlR allows developers to create secure tunnels to their local development servers, making them accessible via public URLs. Built with .NET 9 and clean architecture principles.

## ✨ What Makes TunnlR Different?

- **Persistent URLs** - Your tunnel URL stays the same across reconnections
- **Secure by Default** - JWT authentication for all connections
- **Real-time** - WebSocket-based tunnel connections
- **Self-hostable** - Deploy on your own AWS EC2 instance
- **Clean Architecture** - Maintainable, testable codebase

---

## 🎯 Current Features (MVP)

### ✅ Authentication System
- User registration with email/password
- JWT token-based authentication
- Secure token storage on developer's machine
- Token expiration handling (24-hour validity)

### ✅ Tunnel Management
- One persistent tunnel per user
- Automatic tunnel reuse on reconnection
- Unique subdomain generation (e.g., `abc12345.tunnlr.dev`)
- Tunnel status tracking (Active/Inactive)

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

### ✅ Relay Server (API)
- RESTful authentication endpoints
- WebSocket tunnel endpoint
- Connection pooling and management
- JWT token validation
- SQLite database for data persistence

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
```

### Technology Stack

**Backend (Relay Server):**
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
  }
}
```

### 4. Apply Database Migrations
```bash
cd TunnlR.API
dotnet ef database update
```

### 5. Run the Relay Server
```bash
cd TunnlR.API
dotnet run
```

The API will start on `https://localhost:5001` (or the port configured in `launchSettings.json`)

### 6. Configure the CLI Tool

Update `TunnlR.CLI/appsettings.json`:
```json
{
  "RelayServer": {
    "HttpUrl": "https://relayserver:5001",
    "WebSocketUrl": "wss://websocket:5001"
  }
}
```

### 7. Build the CLI Tool
```bash
cd TunnlR.CLI
dotnet build
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


#### 2. Login

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
- If the token is missing or expired, the user is prompted to log in again before the tunnel can start.

- 

### Starting a Tunnel

Assuming you have a local web server running on port 3000:
```bash
>tunnlr start 3000

Starting tunnel on port 3000...
Connecting [====================]

✅ Connected to relay server!

✅ Tunnel started!
Public URL: https://abc12345.tunnlr.dev
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

### Relay Server Configuration

**File:** `TunnlR.API/appsettings.json`
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
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

- ⚠️ **Traffic forwarding not yet implemented** - Tunnel URLs are generated but HTTP traffic forwarding from internet to localhost is in progress
- ⚠️ **No traffic inspection/logging** - Request/response logging coming in Phase 2
- ⚠️ **Single tunnel per user** - Multiple tunnels support planned for future
- ⚠️ **No custom subdomains** - Random subdomain generation only
- ⚠️ **No HTTPS for local servers** - Only HTTP forwarding currently supported
- ⚠️ **No dashboard UI** - Dashboard URL is a placeholder

---

## 🗺️ Roadmap

### Phase 1: Core Functionality (Current)
- [x] User authentication
- [x] Tunnel creation
- [x] WebSocket connections
- [x] CLI tool
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

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

**Your Name**
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