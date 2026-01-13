# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run Commands

```bash
# Start PostgreSQL database (requires Docker)
docker-compose up -d

# Build the project
dotnet build

# Run the application
dotnet run

# Build for release
dotnet build -c Release

# Reset database (delete and recreate with seed data)
docker-compose down -v && docker-compose up -d
# Then run the app - EnsureCreated() will recreate schema and seed data
```

## Technology Stack

- .NET 9.0 Windows Forms application
- Entity Framework Core 9.0.3 with PostgreSQL database
- PostgreSQL 16 in Docker (docker-compose.yml in project directory)
- Database connection: `Host=localhost;Port=5432;Database=equipment_db;Username=equipment_user;Password=equipment_pass`

## Architecture Overview

This is a TV channel content management application for managing rights owners, films, contacts, and TV schedules. The UI is in Russian.

### Data Layer
- `Data/AppDbContext.cs` - EF Core DbContext with DbSets for all entities
- Database is auto-created on first run with seed data in `Program.cs`
- Uses lazy loading proxies - all navigation properties must be `virtual`
- Connection string is hardcoded in AppDbContext (no config files)

### Models
- `User` - Authentication with Login, Password, RoleId (foreign key to Role)
- `Role` - Permissions system with 14 granular permission flags
- `RightsOwner` - Rights holder (e.g., Беларусьфильм, Paramount)
- `Film` - Film entity with title, age restriction, duration, file path, purchase date, rights expiration, show count
- `Contact` - Contact information for rights sellers
- `TvScheduleEntry` - TV program schedule entries

### Entity Relationships and Cascades
- User → Role: Many-to-one, Restrict delete (cannot delete role with users)
- Film → RightsOwner: Many-to-one, Cascade delete (deleting owner deletes films)
- RightsOwner → Contact: Many-to-one, SetNull (deleting contact nullifies FK)
- TvScheduleEntry → Film: Many-to-one, Cascade delete

### Roles and Permissions
- **Администратор** - Full system access
- **Инженер видеомонтажа** - Creates rights owners, adds films with basic info (title, age, duration, file path)
- **Специалист** - Edits rights info (purchase date, expiration, show count), manages contacts
- **Руководитель** - View-only access

### Forms Architecture
- `LoginForm` - Entry point with logo, authenticates against Users table
- `MainForm` - MDI container with role-based menu navigation and header with logo
- `Forms/CRUD/CrudForm<T>` - Generic abstract base class providing standardized CRUD UI pattern

### Key Business Rules
- Films have `HasValidRights` computed property: rights not expired AND ShowCount > 0
- Films cannot be added to schedule if ShowCount = 0 or rights expired
- TV schedule auto-decrements ShowCount when marking films as aired
- TvScheduleForm has 60-second auto-refresh timer for processing past entries

### Color Coding in UI
- **Red (LightCoral)** - Invalid rights (expired or zero shows)
- **Yellow (LightYellow)** - Low show count (≤5 remaining)

### Date Format
User input uses `dd.MM.yyyy` format parsed with `CultureInfo.InvariantCulture`

### Utils
- `InputDialog` - Reusable prompt dialog for user input
- `SessionManager` - Static singleton for current user session and permission checks

## Default Credentials
- Admin: login=`admin`, password=`admin`
- Video Editor: login=`editor`, password=`editor`
- Specialist: login=`specialist`, password=`specialist`
- Manager: login=`manager`, password=`manager`

## Logo
Place a `logo.png` file in the application directory (or bin/Debug/net9.0-windows/) to display the TV channel logo on the login and main forms.
