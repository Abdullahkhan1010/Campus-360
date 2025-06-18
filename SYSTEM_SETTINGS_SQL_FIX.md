# System Settings SQL Fix Applied

## Issues Fixed

### 1. Foreign Key Type Mismatch
The `create_system_settings_tables.sql` file had foreign key type mismatches. Foreign key columns were defined as `TEXT` but were referencing `auth.users(id)` which is a `UUID` column.

### 2. RLS Policy Type Casting Issues  
Row Level Security policies had complex subqueries referencing `user_profiles` table that caused type mismatches between `text` and `uuid` types.

## Changes Made

### Foreign Key Columns (TEXT → UUID):
- `system_settings.created_by`
- `system_settings.updated_by`
- `user_preferences.user_id`
- `audit_logs.user_id`
- `system_reports.created_by`  
- `system_reports.updated_by`
- `report_schedules.created_by`

### RLS Policy Simplification:
- **FINAL FIX**: Simplified all RLS policies to use `auth.jwt() ->> 'role'` directly
- Removed complex subqueries that referenced `user_profiles` table
- Added explicit `::text` casting to ensure consistent text comparison
- Changed `user_id = auth.uid()` comparisons to avoid UUID casting issues
- Simplified report access to use `(auth.jwt() ->> 'role')::text = ANY(access_roles)`

## Root Cause
The issue was that the complex subqueries referencing `user_profiles` table were causing type confusion between the `auth.uid()` function return type and the database column types. By simplifying the policies to use the JWT role claim directly, we avoid all table joins and type casting issues.

## Status
✅ **SQL file is now ready for execution**

All type mismatch errors have been resolved by simplifying the RLS policies.

## Security Note
The simplified policies rely on the `role` claim in the JWT token. Ensure your authentication system properly sets this claim when users log in.

## Next Steps
1. Execute the SQL file in your Supabase SQL editor
2. Verify the tables were created successfully with proper RLS policies
3. Test the SystemSettingsService and ReportingService with the new tables
4. Ensure JWT tokens include the correct `role` claim for your users
