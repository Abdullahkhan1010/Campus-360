-- Option 3: Manual migration with full control
-- Run this in Supabase SQL Editor

-- Step 1: Analyze current situation
WITH user_analysis AS (
    SELECT 
        up.id,
        up.email,
        up.full_name,
        up.role,
        up.is_verified,
        up.is_active,
        up.created_at,
        CASE 
            WHEN au.id IS NOT NULL THEN 'Has Auth'
            ELSE 'Missing Auth'
        END as auth_status,
        -- Check if email already exists in auth.users with different ID
        CASE 
            WHEN EXISTS(SELECT 1 FROM auth.users WHERE email = up.email AND id != up.id) THEN 'Email Conflict'
            ELSE 'No Conflict'
        END as email_conflict_status
    FROM user_profiles up
    LEFT JOIN auth.users au ON up.id = au.id
)
SELECT 
    auth_status,
    email_conflict_status,
    COUNT(*) as user_count
FROM user_analysis
GROUP BY auth_status, email_conflict_status
ORDER BY auth_status, email_conflict_status;

-- Step 2: Show detailed analysis
SELECT 
    up.id,
    up.email,
    up.full_name,
    up.role,
    CASE 
        WHEN au.id IS NOT NULL THEN 'Has Auth User'
        WHEN EXISTS(SELECT 1 FROM auth.users WHERE email = up.email) THEN 'Email exists with different ID'
        ELSE 'No auth user'
    END as status,
    up.created_at
FROM user_profiles up
LEFT JOIN auth.users au ON up.id = au.id
ORDER BY up.created_at DESC;

-- Step 3: Recommended action for each user
SELECT 
    up.id as current_profile_id,
    up.email,
    up.full_name,
    up.role,
    CASE 
        WHEN au.id IS NOT NULL THEN 'Keep as-is (already has auth)'
        WHEN EXISTS(SELECT 1 FROM auth.users au2 WHERE au2.email = up.email) THEN 'Update profile ID to match existing auth user'
        ELSE 'Create new auth user OR delete and recreate'
    END as recommended_action,
    CASE 
        WHEN EXISTS(SELECT 1 FROM auth.users au2 WHERE au2.email = up.email) 
        THEN (SELECT au2.id FROM auth.users au2 WHERE au2.email = up.email LIMIT 1)
        ELSE NULL
    END as existing_auth_id
FROM user_profiles up
LEFT JOIN auth.users au ON up.id = au.id
ORDER BY up.created_at DESC;

SELECT 'Migration analysis complete. Choose your approach based on the results above.' as status;
