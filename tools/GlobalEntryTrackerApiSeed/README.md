# Global Entry Tracker API Seeder

This is a console application that seeds the database with appointment location data from the CBP (Customs and Border Protection) API.

## Purpose

This tool fetches Global Entry appointment locations from the official CBP API and populates the database with the location data. It's designed to be run as a one-time initialization step or periodically to keep location data up to date.

## Features

- Fetches appointment locations from the CBP Scheduler API
- Filters for US locations only and excludes temporary locations
- Performs upsert operations (creates new locations or updates existing ones)
- Uses proper dependency injection and logging
- Supports both configuration files and environment variables
- Proper error handling with exit codes

## Configuration

The application can be configured using either:

1. **Environment Variables** (recommended for Docker/production):
   - `CONNECTION_STRING` - Full PostgreSQL connection string
   - Or individual variables:
     - `DB_HOST` - Database host
     - `DB_PORT` - Database port (default: 5432)
     - `DB_USER` - Database username
     - `DB_PASSWORD` - Database password
     - `DB_NAME` - Database name

2. **appsettings.json** - Configuration file (for local development)

## Running Locally

```bash
# Set environment variables
export DB_HOST=localhost
export DB_PORT=5432
export DB_USER=youruser
export DB_PASSWORD=yourpassword
export DB_NAME=globalentrytracker

# Run the seeder
dotnet run
```

## Running with Docker

```bash
# Build the image (from repository root)
docker build -f tools/GlobalEntryTrackerApiSeed/Dockerfile -t globalentry-seeder .

# Run with environment variables
docker run --rm \
  -e DB_HOST=your-db-host \
  -e DB_PORT=5432 \
  -e DB_USER=youruser \
  -e DB_PASSWORD=yourpassword \
  -e DB_NAME=globalentrytracker \
  globalentry-seeder
```

## Exit Codes

- `0` - Success
- `1` - Error (check logs for details)

## Architecture

The seeder follows clean architecture principles:

- **Program.cs** - Entry point, sets up dependency injection and configuration
- **Services/AppointmentLocationSeederService.cs** - Core business logic
- **Models/AppointmentLocation.cs** - API response model
- **Database project reference** - Shared entities and DbContext

## Dependencies

- Database project (shared entities and DbContext)
- Microsoft.Extensions.Hosting (dependency injection and configuration)
- Microsoft.EntityFrameworkCore (database access)
- Npgsql.EntityFrameworkCore.PostgreSQL (PostgreSQL provider)

## Notes

- The seeder is safe to run multiple times - it will update existing records rather than creating duplicates
- Only US locations that are not temporary are seeded
- All string fields are trimmed and truncated to match database column constraints

