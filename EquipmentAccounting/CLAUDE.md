# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run Commands

```bash
# Start PostgreSQL database (requires Docker)
cd .. && docker-compose up -d

# Build the project
dotnet build

# Run the application
dotnet run

# Build for release
dotnet build -c Release
```

## Technology Stack

- .NET 9.0 Windows Forms application
- Entity Framework Core 9.0.3 with PostgreSQL database
- PostgreSQL 16 in Docker (docker-compose.yml in parent directory)
- Database connection: `Host=localhost;Port=5432;Database=equipment_db;Username=equipment_user;Password=equipment_pass`

## Architecture Overview

This is a TV channel content management application for managing rights owners, films, contacts, and TV schedules. The UI is in Russian.

### Data Layer
- `Data/AppDbContext.cs` - EF Core DbContext with DbSets for all entities
- Database is auto-created on first run with seed data in `Program.cs`
- Uses lazy loading proxies for navigation properties

### Models
- `User` - Authentication with Login, Password, RoleId (foreign key to Role)
- `Role` - Permissions system with granular access controls
- `RightsOwner` - Rights holder (e.g., Беларусьфильм, Paramount)
- `Film` - Film entity with title, age restriction, duration, file path, purchase date, rights expiration, show count
- `Contact` - Contact information for rights sellers
- `TvScheduleEntry` - TV program schedule entries

### Roles and Permissions
- **Администратор** - Full system access
- **Инженер видеомонтажа** - Creates rights owners, adds films with basic info (title, age, duration, file path)
- **Специалист** - Edits rights info (purchase date, expiration, show count), manages contacts
- **Руководитель** - View-only access

### Forms Architecture
- `LoginForm` - Entry point with logo, authenticates against Users table
- `MainForm` - MDI container with role-based menu navigation and header with logo
- `Forms/CRUD/CrudForm<T>` - Generic abstract base class providing standardized CRUD UI

### Menu Structure
- **Контент** - Rights owners list, opens films for each owner
- **Контакты** - Contact management
- **Программа** - TV schedule with show count tracking
- **Администрирование** - User and role management (admin only)
- **Справка** - About dialog

### Key Features
- Role-based access control (RBAC) with granular permissions
- TV schedule with automatic show count decrement when films are aired
- Films cannot be added to schedule if show count is 0 or rights have expired
- Color-coded display for expired rights and low show counts
- Replaceable TV channel logo (`logo.png` in output directory)

### Utils
- `InputDialog` - Reusable prompt dialog for user input
- `SessionManager` - Static class for managing current user session and permission checks

## Default Credentials
- Admin: login=`admin`, password=`admin`
- Video Editor: login=`editor`, password=`editor`
- Specialist: login=`specialist`, password=`specialist`
- Manager: login=`manager`, password=`manager`

## Logo
Place a `logo.png` file in the application directory (or bin/Debug/net9.0-windows/) to display the TV channel logo on the login and main forms.
