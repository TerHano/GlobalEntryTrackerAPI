# Global Entry Tracker API Seeder

This is a console application that seeds the database with appointment location data from the CBP (Customs and Border
Protection) API.

## Purpose

This tool fetches Global Entry appointment locations from the official CBP API and populates the database with the
location data. It's designed to be run as a one-time initialization step or periodically to keep location data up to
date.

## Features

- Fetches appointment locations from the CBP Scheduler API
- Filters for US locations only and excludes temporary locations
- Performs upsert operations (creates new locations or updates existing ones)
- Seeds notification types (Weekend and Soonest) into the database
- Seeds admin user with credentials from environment variables
- Optional Stripe subscription catalog sync into `PlanOptions`
- Optional Stripe subscriber backfill into `UserCustomers` + Subscriber role
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
   - `DRY_RUN` - Optional, default `false` (preview mode with no DB writes)

- `REPORT_PATH` - Optional, default `seed-report.json` (dry-run JSON output file)

2. **Stripe Catalog Seeding Variables** (required only for Stripe mode):
   - `STRIPE_SECRET_KEY` - Stripe secret key (test or live)
   - `STRIPE_ONLY_ACTIVE` - Optional, default `true`
   - `STRIPE_PRODUCT_IDS` - Optional CSV filter by Stripe product IDs
   - `STRIPE_PRICE_IDS` - Optional CSV filter by Stripe price IDs

3. **Stripe Subscriber Backfill Variables** (required only for backfill mode):
   - `STRIPE_SECRET_KEY` - Stripe secret key (test or live)
   - `STRIPE_BACKFILL_STATUSES` - Optional CSV, default `active,trialing`
   - `STRIPE_BACKFILL_MATCH_EMAIL` - Optional, default `true` (fallback matching when metadata userId is absent)

4. **Admin User Seeding Variables** (required only for admin user mode):
   - `ADMIN_EMAIL` - Required, email address for the admin user
   - `ADMIN_PASSWORD` - Required, password for the admin user
   - `ADMIN_FIRST_NAME` - Optional, first name for the admin user (default: "Admin")
   - `ADMIN_LAST_NAME` - Optional, last name for the admin user (default: "User")

5. **appsettings.json** - Configuration file (for local development)

## Running Locally

```bash
# Set environment variables
export DB_HOST=localhost
export DB_PORT=5432
export DB_USER=youruser
export DB_PASSWORD=yourpassword
export DB_NAME=globalentrytracker

# Seed appointment locations (default mode)
dotnet run

# Explicitly seed locations
dotnet run -- --seed-locations

# Preview location changes without writing to DB
dotnet run -- --seed-locations --dry-run

# Seed notification types
dotnet run -- --seed-notification-types

# Preview notification type seeding without writing to DB
dotnet run -- --seed-notification-types --dry-run

# Seed roles
dotnet run -- --seed-roles

# Preview role seeding without writing to DB
dotnet run -- --seed-roles --dry-run

# Seed admin user
export ADMIN_EMAIL=admin@example.com
export ADMIN_PASSWORD=SecurePassword123!
export ADMIN_FIRST_NAME=Super
export ADMIN_LAST_NAME=Admin
dotnet run -- --seed-admin-user

# Preview admin user seeding without writing to DB
dotnet run -- --seed-admin-user --dry-run

# Seed Stripe subscription catalog into PlanOptions
export STRIPE_SECRET_KEY=sk_test_xxx
dotnet run -- --seed-stripe-catalog

# Preview Stripe catalog upserts without writing to DB
dotnet run -- --seed-stripe-catalog --dry-run

# Backfill Stripe subscribers into UserCustomers and assign Subscriber role
export STRIPE_SECRET_KEY=sk_test_xxx
dotnet run -- --backfill-stripe-subscribers

# Preview subscriber backfill matches/role updates without writing to DB
dotnet run -- --backfill-stripe-subscribers --dry-run

# Run multiple modes in one execution
dotnet run -- --seed-locations --seed-notification-types --seed-roles --seed-admin-user --seed-stripe-catalog --backfill-stripe-subscribers --dry-run

# Optional: write dry-run report to a custom path
export REPORT_PATH=./reports/my-seed-report.json
dotnet run -- --seed-stripe-catalog --dry-run
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
- **Services/AppointmentLocationSeederService.cs** - Location seeding logic
- **Services/NotificationTypeSeederService.cs** - Notification type seeding logic
- **Services/RoleSeederService.cs** - Role seeding logic
- **Services/AdminUserSeederService.cs** - Admin user seeding logic
- **Services/StripeCatalogSeederService.cs** - Stripe plan catalog seeding logic
- **Services/StripeSubscriberBackfillService.cs** - Stripe subscription-to-user backfill logic
- **Models/AppointmentLocation.cs** - API response model
- **Models/StripeCatalogSeedOptions.cs** - Stripe seed options model
- **Models/StripeSubscriberBackfillOptions.cs** - Stripe backfill options model
- **Database project reference** - Shared entities and DbContext

## Dependencies

- Database project (shared entities and DbContext)
- Microsoft.Extensions.Hosting (dependency injection and configuration)
- Microsoft.EntityFrameworkCore (database access)
- Npgsql.EntityFrameworkCore.PostgreSQL (PostgreSQL provider)

## Notes

- The seeder is safe to run multiple times - it will update existing records rather than creating duplicates
- Only US locations that are not temporary are seeded
- Notification types (Weekend and Soonest) are seeded based on the NotificationType enum
- Admin user seeding checks if the user already exists by email and skips creation if found
- Admin user's email is automatically confirmed upon creation
- Admin role must exist in the database before seeding an admin user (roles are seeded on app startup)
- All string fields are trimmed and truncated to match database column constraints
- Stripe catalog seeding is intended for subscription plan metadata (`PlanOptions`), not live subscription state
- Stripe subscriber backfill should be used as a one-time migration/recovery utility; webhook sync remains the long-term
  source for ongoing entitlement updates
- Any dry-run execution writes a structured JSON report (default `seed-report.json`) with per-mode summary counts
