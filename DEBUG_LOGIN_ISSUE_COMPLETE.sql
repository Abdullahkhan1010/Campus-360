-- ========================================
-- COMPREHENSIVE LOGIN DEBUG
-- ========================================
-- This will help us identify exactly why the user profile lookup is failing

-- Step 1: Check if the admin user exists in user_profiles
SELECT 'Step 1: Checking if admin user exists in user_profiles' as debug_step;
SELECT COUNT(*) as admin_profile_count FROM user_profiles WHERE email = 'admin@campus360.com';

-- Step 2: Show the actual admin user data (if it exists)
SELECT 'Step 2: Admin user profile data' as debug_step;
SELECT id, email, full_name, role, is_verified, is_active FROM user_profiles WHERE email = 'admin@campus360.com';

-- Step 3: Check if there's an auth user for this email
SELECT 'Step 3: Checking auth.users table' as debug_step;
SELECT COUNT(*) as auth_user_count FROM auth.users WHERE email = 'admin@campus360.com';
SELECT id, email, created_at FROM auth.users WHERE email = 'admin@campus360.com';

-- Step 4: Check RLS status on user_profiles
SELECT 'Step 4: RLS status on user_profiles' as debug_step;
SELECT schemaname, tablename, rowsecurity as rls_enabled FROM pg_tables WHERE tablename = 'user_profiles';

-- Step 5: Show all current RLS policies on user_profiles
SELECT 'Step 5: Current RLS policies on user_profiles' as debug_step;
SELECT policyname, cmd, permissive, qual FROM pg_policies WHERE tablename = 'user_profiles';

-- Step 6: Test direct access to user_profiles (bypass any app-level filtering)
SELECT 'Step 6: All users in user_profiles table' as debug_step;
SELECT email, full_name, role FROM user_profiles ORDER BY created_at;

-- Step 7: Check current authentication context
SELECT 'Step 7: Current auth context' as debug_step;
SELECT 
    auth.uid() as current_user_id,
    auth.role() as current_role,
    CASE WHEN auth.uid() IS NULL THEN 'Not authenticated' ELSE 'Authenticated' END as auth_status;

-- Step 8: Test if we can access user_profiles with RLS disabled
ALTER TABLE user_profiles DISABLE ROW LEVEL SECURITY;
SELECT 'Step 8: Testing with RLS disabled' as debug_step;
SELECT COUNT(*) as total_users_without_rls FROM user_profiles;
SELECT email FROM user_profiles WHERE email = 'admin@campus360.com';
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;

SELECT 'Debug analysis complete!' as status;
