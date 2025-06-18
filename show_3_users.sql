-- Show details of your 3 users
-- Run this in Supabase SQL Editor

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

-- Also check if any have auth users
SELECT 
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
