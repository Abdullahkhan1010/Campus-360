-- Check existing users and their status
-- Run this in Supabase SQL Editor

-- Check current user_profiles
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

-- Check if any of these users exist in auth.users
SELECT 
    up.id as profile_id,
    up.email,
    up.full_name,
    up.role,
    CASE 
        WHEN au.id IS NOT NULL THEN 'Has Auth User'
        ELSE 'Missing Auth User'
    END as auth_status
FROM user_profiles up
LEFT JOIN auth.users au ON up.id = au.id
ORDER BY up.created_at DESC;

-- Count users by auth status
SELECT 
    CASE 
        WHEN au.id IS NOT NULL THEN 'Has Auth User'
        ELSE 'Missing Auth User'
    END as auth_status,
    COUNT(*) as user_count
FROM user_profiles up
LEFT JOIN auth.users au ON up.id = au.id
GROUP BY (au.id IS NOT NULL);

SELECT 'Current user status analysis complete' as status;
