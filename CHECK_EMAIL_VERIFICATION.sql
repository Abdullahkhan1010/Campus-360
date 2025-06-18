-- ===================================================================
-- DISABLE EMAIL VERIFICATION IN SUPABASE
-- ===================================================================
-- Run this in Supabase SQL Editor to check/disable email verification
-- ===================================================================

-- 1. Check current auth configuration
SELECT 
    'Current auth settings:' as info;

-- Check if there are any auth configuration settings
SELECT 
    name,
    value,
    description
FROM pg_settings 
WHERE name LIKE '%confirm%' OR name LIKE '%email%'
ORDER BY name;

-- 2. Check auth.users table to see email confirmation status
SELECT 
    'Users email confirmation status:' as info;

SELECT 
    email,
    email_confirmed_at,
    confirmation_token,
    created_at,
    CASE 
        WHEN email_confirmed_at IS NOT NULL THEN 'Confirmed'
        ELSE 'Not Confirmed'
    END as status
FROM auth.users
ORDER BY created_at DESC
LIMIT 10;

-- 3. Create a test user that should work without email verification
-- This will work if email verification is disabled at project level
SELECT 'Creating test user without email verification...' as info;

-- Note: You cannot directly insert into auth.users from SQL
-- You need to disable email confirmation in Supabase Dashboard:
-- 1. Go to Authentication > Settings
-- 2. Turn OFF "Enable email confirmations"
-- 3. Save settings

SELECT 'To disable email verification:' as instructions;
SELECT '1. Go to Supabase Dashboard > Authentication > Settings' as step1;
SELECT '2. Find "Enable email confirmations" and turn it OFF' as step2;
SELECT '3. Save the settings' as step3;
SELECT '4. Try creating users again - they should work immediately' as step4;

-- 4. Check if there are any email-related policies that might interfere
SELECT 
    'Checking for email-related policies:' as info;

SELECT 
    schemaname,
    tablename,
    policyname,
    permissive,
    roles,
    cmd,
    qual,
    with_check
FROM pg_policies 
WHERE qual LIKE '%email%' OR with_check LIKE '%email%'
ORDER BY schemaname, tablename;

SELECT 'EMAIL VERIFICATION CHECK COMPLETED!' as status;
