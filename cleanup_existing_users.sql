-- Option 2: Clean slate approach - remove existing users and recreate them properly
-- Run this in Supabase SQL Editor
-- WARNING: This will delete existing user data!

-- First, let's backup existing users
CREATE TABLE IF NOT EXISTS user_profiles_backup AS 
SELECT * FROM user_profiles;

-- Show what we're about to delete
SELECT 
    id,
    email,
    full_name,
    role,
    'Will be deleted and recreated' as action
FROM user_profiles
ORDER BY created_at;

-- Count of users to be affected
SELECT COUNT(*) as users_to_recreate FROM user_profiles;

-- Delete existing user profiles (uncomment when ready)
-- DELETE FROM user_profiles;

-- Verify backup was created
SELECT COUNT(*) as backup_count FROM user_profiles_backup;

SELECT 'Backup created. Uncomment DELETE statement when ready to proceed.' as status;

-- After running this, you can recreate users through the application
-- They will be created with proper auth integration
