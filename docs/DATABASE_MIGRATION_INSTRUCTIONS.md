# Database Migration Instructions

## Critical: Initial Migration Required

The database schema is fully defined but NO migrations have been created yet. This must be done before deployment.

## Step 1: Create Initial Migration

```bash
cd backend/src/HomeService.API

dotnet ef migrations add InitialCreate \
  --project ../HomeService.Infrastructure \
  --startup-project . \
  --output-dir Data/Migrations
```

## Step 2: Review Generated Migration

Check the generated migration file in `HomeService.Infrastructure/Data/Migrations/` to ensure all entities are included:
- Users
- ServiceProviders
- ServiceCategories
- Services
- Bookings
- Payments
- Reviews
- Addresses
- Notifications
- ProviderAvailability

## Step 3: Apply Migration to Development Database

```bash
dotnet ef database update --project ../HomeService.Infrastructure --startup-project .
```

## Step 4: Verify Database

Connect to SQL Server and verify:
```sql
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

Expected tables: 10 entity tables + __EFMigrationsHistory

## Future Migrations

For any schema changes:
```bash
dotnet ef migrations add <MigrationName> \
  --project ../HomeService.Infrastructure \
  --startup-project .
```

## Production Deployment

Always:
1. Review migration SQL before applying
2. Backup database before migration
3. Test on staging first
4. Have rollback plan ready

## Connection String

Update in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HomeServiceDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```
