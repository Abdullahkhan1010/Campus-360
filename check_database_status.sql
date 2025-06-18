-- Quick check to see what tables exist in your Supabase database
-- Run this in Supabase SQL Editor to verify tables were created

SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;

-- Also check if user_profiles table exists specifically
SELECT EXISTS (
    SELECT FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_name = 'user_profiles'
) as user_profiles_exists;

-- Check if there are any users in the auth.users table
SELECT count(*) as auth_users_count FROM auth.users;

-- Check if there are any records in user_profiles (if it exists)
SELECT count(*) as user_profiles_count FROM user_profiles;
