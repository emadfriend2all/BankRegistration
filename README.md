# Registration Portal - Complete Setup and Deployment Guide

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Database Management](#database-management)
3. [Development Setup](#development-setup)
4. [Production Deployment](#production-deployment)
5. [IIS Setup Step-by-Step](#iis-setup-step-by-step)
6. [Production Configuration](#production-configuration)
7. [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Software
- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** - [Download here](https://nodejs.org/)
- **Oracle Database 11g+** or Oracle Database XE
- **Oracle Data Provider for .NET** - [ODP.NET Managed Driver](https://www.oracle.com/database/technologies/dotnet.html)
- **IIS** (Windows Server/Professional)
- **ASP.NET Core Hosting Bundle** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)

### Development Tools
- Visual Studio 2022 or VS Code
- Git for source control
- Oracle SQL Developer or similar tool

## Database Management

### Connection String Configuration
Update `appsettings.json` with your Oracle connection details:

```json
{
  "ConnectionStrings": {
    "OracleConnection": "Data Source=YOUR_ORACLE_HOST:1521/YOUR_SID;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;Pooling=true;Min Pool Size=5;Max Pool Size=100;Connection Timeout=30;"
  }
}
```

### Running Migrations

#### Automatic Migration (Recommended)
The application automatically runs migrations on startup:

```bash
# The application will:
# 1. Check if database exists
# 2. Create database if needed
# 3. Run all pending migrations
# 4. Seed initial data
```

#### Manual Migration Commands
```bash
# Create a new migration
cd RegistrationPortal.Server
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove
```

#### Database Updates
When you make entity changes:
1. Create migration: `dotnet ef migrations add Description`
2. Test locally: `dotnet ef database update`
3. Deploy - migrations run automatically in production

### Database Seeding
The application automatically seeds:
- Default roles and permissions
- Admin user (check SeederService for details)
- Initial configuration data

## Development Setup

### 1. Clone Repository
```bash
git clone <repository-url>
cd RegistrationPortal
```

### 2. Backend Setup
```bash
cd RegistrationPortal.Server
dotnet restore
dotnet build
```

### 3. Frontend Setup
```bash
cd RegistrationPortaltral.Client
npm install
```

### 4.place Configuration
```bash
# Developmentamend appsettings.json with your Oracle海外的connection string
odata
# Update public/config/development.json with your API settings
```

### 5. Run Development Environment
```bash
# Terminal 1 - Backend
cd RegistrationPortal.Server
dotnet run

# Terminal 2 - Frontend
cd RegistrationPortal.Client
ng serve --proxy-config public/config/development.json
```

Access application at: http://localhost:4200

## Production Deployment

### Build Script (Recommended)
Use the provided build script for automated deployment:

```bash
# Build for production
.\build-for-iis.bat
```

This script:
1. Builds Angular for production
2. Builds .NET server for production
3. Combines both into `PublishedApp` folder
4. Flattens browser folder structure
5. Creates deployment-ready package

### Manual Build Steps

#### 1. Build Angular
```bash
cd RegistrationPortal.Client
npm run build -- --configuration production
```

#### 2. Build .NET
```bash
cd RegistrationPortal.Server
dotnet publish -c Release -o ../PublishedApp
```

#### 3. Flatten Browser Folder
```bash
cd PublishedApp
robocopy "wwwroot/browser" "wwwroot" /E /MOVE
rmdir /s /q "wwwroot/browser"
```

## IIS Setup Step-by-Step

### Step 1: Install IIS
1. **Windows Server:**
   - Open Server Manager
   - Add Roles → Web Server (IIS)
   - Include: ASP.NET Core Hosting Bundle, URL Rewrite

2. **Windows 10/11 Professional:**
   - Turn Windows features on/off
   - Internet Information Services
   - Check: ASP.NET Core Hosting Bundle

### Step 2: Install ASP.NET Core Hosting Bundle
1. Download from: https://dotnet.microsoft.com/download/dotnet/8.0
2. Run installer as Administrator
3. Restart IIS: `iisreset`

### Step 3: Create Application Pool
1. Open IIS Manager
2. Right-click "Application Pools" → Add Application Pool
3. Name: `RegistrationPortalPool`
4. .NET CLR Version: **No Managed Code**
5. Managed Pipeline Mode: Integrated
6. Click OK

### Step 4: Create Website
1. Right-click "Sites" → Add Website
2. Site name: `RegistrationPortal`
3. Application pool: `RegistrationPortalPool`
4. Physical path: `D:\Sites\RegistrationPortal` (or your path)
5. Port: `80` (HTTP) or `443` (HTTPS)
6. Click OK

### Step 5: Deploy Application Files
1. Copy `PublishedApp` contents to your IIS site folder
2. Ensure `web.config` is in the root
3. Set folder permissions:
   - IIS_IUSRS: Read & Execute
   - Application Pool Identity: Full control

### Step 6: Configure Application
1. Select your website in IIS Manager
2. Double-click "Configuration Editor"
3. Section: `system.webServer/aspNetCore`
4. Set `environmentVariables`:
   - Name: `ASPNETCORE_ENVIRONMENT`
   - Value: `Production`

### Step 7: Test Deployment
1. Browse to: `http://localhost`
2. Check application logs if issues occur
3. Verify database connectivity

## Production Configuration

### Server Configuration Files

#### appsettings.Production.json
```json
{
  "ServerUrl": "http://your-domain.com",
  "ConnectionStrings": {
    "OracleConnection": "Production connection string"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

#### web.config (IIS)
The application includes a production-ready web.config with:
- ASP.NET Core handler configuration
- Production environment variable
- Error handling settings

### Frontend Configuration

#### public/config/production.json
```json
{
  "api": {
    "baseUrl": "",
    "proxy": {
      "/api/*": {
        "target": "http://localhost",
        "secure": false,
        "changeOrigin": true,
        "logLevel": "error"
      }
    }
  },
  "app": {
    "rtl": true,
    "darkModeSelector": ".app-dark",
    "theme": "aura"
  }
}
```

### Environment Detection
The application automatically detects production environment:
- `localhost:80` → Production
- Any non-localhost domain → Production
- `localhost:4200` → Development

## Production Changes Checklist

### Before Deployment
- [ ] Update connection strings in production config
- [ ] Test migrations on staging environment
- [ ] Update ServerUrl in appsettings.Production.json
- [ ] Review CORS policies if needed
- [ ] Backup production database

### After Deployment
- [ ] Verify database connectivity
- [ ] Test all API endpoints
- [ ] Check frontend functionality
- [ ] Monitor application logs
- [ ] Test user authentication

### Security Considerations
- [ ] Change default passwords
- [ ] Configure HTTPS/SSL certificates
- [ ] Review file permissions
- [ ] Set up logging and monitoring
- [ ] Configure backup strategies

## Troubleshooting

### Common Issues

#### HTTP Error 500.19
**Issue:** Invalid web.config
**Solution:** Ensure ASP.NET Core Hosting Bundle is installed

#### HTTP Error 500.30
**Issue:** ANCM in-process failure
**Solution:** Check application pool settings and .NET runtime

#### Database Connection Issues
**Issue:** Oracle connection fails
**Solution:** 
- Verify connection string
- Check Oracle client installation
- Test connectivity with SQL Developer

#### Angular 404 Errors
**Issue:** Client-side routing not working
**Solution:** Ensure `MapFallbackToFile("/index.html")` is in Program.cs

#### Migration Failures
**Issue:** ORA-00955 (object already exists)
**Solution:** Application handles this gracefully - continues startup

### Log Locations
- **IIS Logs:** `C:\inetpub\logs\LogFiles\`
- **Application Logs:** Windows Event Viewer → Application
- **.NET Logs:** PublishedApp\logs\ (if enabled)

### Performance Optimization
- Enable output caching in IIS
- Configure database connection pooling
- Use CDN for static assets in production
- Monitor memory usage and CPU

## Support and Maintenance

### Regular Tasks
1. **Database Backups** - Daily automated backups
2. **Log Monitoring** - Check for errors weekly
3. **Security Updates** - Apply Windows and .NET updates
4. **Performance Monitoring** - Monitor response times

### Emergency Procedures
1. **Application Crash** - Check Event Viewer, restart app pool
2. **Database Issues** - Verify connectivity, run manual migrations
3. **Performance Issues** - Check IIS logs, monitor resources

### Contact Information
- **Development Team:** [Contact details]
- **Database Admin:** [Contact details]
- **System Admin:** [Contact details]

---

## Quick Reference Commands

### Development
```bash
# Start backend
cd RegistrationPortal.Server && dotnet run

# Start frontend
cd RegistrationPortal.Client && ng serve

# Create migration
dotnet ef migrations add MigrationName

# Apply migration
dotnet ef database update
```

### Production
```bash
# Build for deployment
.\build-for-iis.bat

# Manual publish
dotnet publish -c Release -o PublishedApp

# Restart IIS
iisreset
```

### Database
```bash
# Check connection
sqlplus username/password@//host:port/sid

# Run manual migration
dotnet ef database update
```

---

**Last Updated:** December 2025
**Version:** 1.0.0
