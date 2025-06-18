-- ===================================================================
-- TEST USER CREATION COMPLETE VERIFICATION
-- ===================================================================
-- Run this to verify user creation is working in both auth and profiles
-- ===================================================================

-- 1. Check current auth users
SELECT 'AUTH USERS:' as section;
SELECT 
    id,
    email,
    email_confirmed_at,
    created_at,
    updated_at,
    CASE 
        WHEN email_confirmed_at IS NOT NULL THEN 'Confirmed'
        ELSE 'Not Required (Good!)'
    END as email_status
FROM auth.users
ORDER BY created_at DESC
LIMIT 5;

-- 2. Check current user profiles
SELECT 'USER PROFILES:' as section;
SELECT 
    id,
    email,
    full_name,
    role,
    department_id,
    is_verified,
    is_active,
    created_at
FROM user_profiles
ORDER BY created_at DESC
LIMIT 5;

-- 3. Check if auth users have corresponding profiles
SELECT 'AUTH/PROFILE MATCHING:' as section;
SELECT 
    a.id as auth_id,
    a.email as auth_email,
    p.id as profile_id,
    p.email as profile_email,
    p.full_name,
    p.role,
    CASE 
        WHEN p.id IS NOT NULL THEN 'Profile Exists'
        ELSE 'Missing Profile'
    END as status
FROM auth.users a
LEFT JOIN user_profiles p ON a.id = p.id
ORDER BY a.created_at DESC
LIMIT 5;

-- 4. Check for orphaned profiles (profiles without auth users)
SELECT 'ORPHANED PROFILES:' as section;
SELECT 
    p.id,
    p.email,
    p.full_name,
    p.role,
    'No Auth User' as issue
FROM user_profiles p
LEFT JOIN auth.users a ON p.id = a.id
WHERE a.id IS NULL;

-- 5. Summary stats
SELECT 'SUMMARY:' as section;
SELECT 
    (SELECT COUNT(*) FROM auth.users) as total_auth_users,
    (SELECT COUNT(*) FROM user_profiles) as total_profiles,
    (SELECT COUNT(*) FROM auth.users a JOIN user_profiles p ON a.id = p.id) as matched_users,
    (SELECT COUNT(*) FROM auth.users a LEFT JOIN user_profiles p ON a.id = p.id WHERE p.id IS NULL) as auth_without_profile,
    (SELECT COUNT(*) FROM user_profiles p LEFT JOIN auth.users a ON p.id = a.id WHERE a.id IS NULL) as profile_without_auth;

SELECT 'TEST COMPLETED - Check results above!' as status;
LIMIT 5;

-- Check if there are auth users that match our profiles
SELECT 
    'AUTH TO PROFILE MATCH' as test_type,
    au.id as auth_id,
    au.email as auth_email,
    up.id as profile_id,
    up.email as profile_email,
    CASE 
        WHEN up.id IS NOT NULL THEN 'MATCHED' 
        ELSE 'NO PROFILE' 
    END as match_status
FROM auth.users au
LEFT JOIN user_profiles up ON au.id = up.id
ORDER BY au.created_at DESC
LIMIT 5;

-- Check for orphaned profiles (profiles without auth users)
SELECT 
    'ORPHANED PROFILES' as test_type,
    up.id,
    up.email,
    up.full_name,
    up.role
FROM user_profiles up
LEFT JOIN auth.users au ON up.id = au.id
WHERE au.id IS NULL
LIMIT 5;

-- Test if we can insert a user profile directly (should work with RLS disabled)
DO $$
DECLARE
    test_user_id uuid := gen_random_uuid();
    test_email text := 'test_' || extract(epoch from now()) || '@example.com';
BEGIN
    BEGIN
        INSERT INTO user_profiles (
            id,
            email,
            full_name,
            role,
            department_id,
            is_verified,
            is_active,
            created_at,
            updated_at
        ) VALUES (
            test_user_id,
            test_email,
            'Test User Direct Insert',
            'student',
            (SELECT id FROM departments LIMIT 1),
            true,
            true,
            NOW(),
            NOW()
        );
        
        RAISE NOTICE 'SUCCESS: Direct insert test passed. User ID: %', test_user_id;
        
        -- Clean up the test user
        DELETE FROM user_profiles WHERE id = test_user_id;
        RAISE NOTICE 'Test user cleaned up successfully';
        
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'ERROR: Direct insert test failed: %', SQLERRM;
    END;
END $$;

-- Final summary
SELECT 
    'FINAL SUMMARY' as test_type,
    (SELECT COUNT(*) FROM user_profiles) as total_profiles,
    (SELECT COUNT(*) FROM auth.users) as total_auth_users,
    (SELECT COUNT(*) FROM user_profiles up INNER JOIN auth.users au ON up.id = au.id) as matched_users,
    (SELECT COUNT(*) FROM departments) as total_departments;
