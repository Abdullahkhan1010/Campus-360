-- Test different ways to query the user_profiles table
-- Run this in Supabase SQL Editor to debug the table access

-- 1. Check if the table exists (case sensitive)
SELECT EXISTS (
    SELECT FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_name = 'user_profiles'
) as lowercase_exists;

SELECT EXISTS (
    SELECT FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_name = 'UserProfileDb'
) as camelcase_exists;

-- 2. Check actual table name as stored
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name ILIKE '%user%profile%';

-- 3. Try to select from the table directly
SELECT * FROM user_profiles LIMIT 1;

-- 4. Check if there are any auth users
SELECT id, email, created_at FROM auth.users;
