-- Show detailed results of user analysis
-- Run this in Supabase SQL Editor to see your actual users

-- Show all current users and their details
SELECT 
    id,
    email,
    full_name,
    role,
    is_verified,
    is_active,
    created_at::date as created_date,
    CASE 
        WHEN id IN (SELECT id FROM auth.users) THEN '✅ Has Auth User'
        ELSE '❌ Missing Auth User'
    END as auth_status
FROM user_profiles 
ORDER BY created_at DESC;

-- Count by role and auth status
SELECT 
    role,
    CASE 
        WHEN id IN (SELECT id FROM auth.users) THEN 'Has Auth'
        ELSE 'Missing Auth'
    END as auth_status,
    COUNT(*) as count
FROM user_profiles 
GROUP BY role, (id IN (SELECT id FROM auth.users))
ORDER BY role, auth_status;

-- Show total counts
SELECT 
    COUNT(*) as total_users,
    COUNT(CASE WHEN role = 'admin' THEN 1 END) as admin_count,
    COUNT(CASE WHEN role = 'teacher' THEN 1 END) as teacher_count,
    COUNT(CASE WHEN role = 'student' THEN 1 END) as student_count,
    COUNT(CASE WHEN is_verified THEN 1 END) as verified_count,
    COUNT(CASE WHEN is_active THEN 1 END) as active_count
FROM user_profiles;

-- Check for duplicate emails
SELECT 
    email, 
    COUNT(*) as count,
    STRING_AGG(id::text, ', ') as user_ids
FROM user_profiles 
GROUP BY email 
HAVING COUNT(*) > 1;

SELECT 'Detailed user analysis complete - review the results above' as status;
