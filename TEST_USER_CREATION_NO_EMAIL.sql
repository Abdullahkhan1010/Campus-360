-- ===================================================================
-- TEST USER CREATION WITHOUT EMAIL VERIFICATION
-- ===================================================================
-- Run this after disabling email verification in Supabase Dashboard
-- ===================================================================

-- 1. First, let's check current auth users
SELECT 
    'Current auth users:' as info,
    COUNT(*) as count
FROM auth.users;

-- Show existing users
SELECT 
    id,
    email,
    email_confirmed_at,
    created_at,
    CASE 
        WHEN email_confirmed_at IS NOT NULL THEN 'Confirmed'
        ELSE 'Pending'
    END as email_status
FROM auth.users
ORDER BY created_at DESC;

-- 2. Check user profiles
SELECT 
    'Current user profiles:' as info,
    COUNT(*) as count
FROM user_profiles;

-- Show existing profiles
SELECT 
    id,
    email,
    full_name,
    role,
    is_verified,
    is_active,
    created_at
FROM user_profiles
ORDER BY created_at DESC;

-- 3. Instructions for testing
SELECT 'TESTING INSTRUCTIONS:' as title;
SELECT '1. Make sure email verification is disabled in Supabase Dashboard' as step1;
SELECT '2. Go to your Campus360 application' as step2;
SELECT '3. Try to create a new user through Admin > Users' as step3;
SELECT '4. The user should be created without requiring email verification' as step4;
SELECT '5. Try to log in with the new user immediately' as step5;

-- 4. If needed, manually create a test user profile for an existing auth user
-- Uncomment the following lines and replace with actual auth user ID

/*
INSERT INTO user_profiles (
    id,
    email,
    full_name,
    role,
    is_verified,
    is_active,
    created_at,
    updated_at
) VALUES (
    '00000000-0000-0000-0000-000000000001', -- Replace with actual auth user ID
    'test@campus360.com',
    'Test User',
    'student',
    true,
    true,
    NOW(),
    NOW()
) ON CONFLICT (email) DO UPDATE SET
    full_name = EXCLUDED.full_name,
    role = EXCLUDED.role,
    is_verified = EXCLUDED.is_verified,
    updated_at = NOW();
*/

SELECT 'TEST PREPARATION COMPLETED!' as status;
