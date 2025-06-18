-- SAFE CLEANUP: Backup and clear user_profiles for fresh start
-- Run this in Supabase SQL Editor

-- Step 1: Create backup (just in case)
DROP TABLE IF EXISTS user_profiles_backup_june12;
CREATE TABLE user_profiles_backup_june12 AS SELECT * FROM user_profiles;

-- Step 2: Check what's in backup
SELECT COUNT(*) as backed_up_users FROM user_profiles_backup_june12;

-- Step 3: Show what we're about to delete (just to see)
SELECT COUNT(*) as current_users FROM user_profiles;

-- Step 4: Clear the table (uncomment when ready)
-- DELETE FROM user_profiles;

-- Step 5: Verify it's empty (run after DELETE)
-- SELECT COUNT(*) as remaining_users FROM user_profiles;

SELECT 'Backup created. Uncomment DELETE statement when ready to clear users.' as status;
