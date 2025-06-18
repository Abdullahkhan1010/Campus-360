-- ========================================
-- FIX USER_PROFILES RLS POLICIES FOR LOGIN
-- ========================================
-- This script ensures that user_profiles table has proper RLS policies for login

-- 1. Drop all existing policies on user_profiles
DROP POLICY IF EXISTS "allow_all_authenticated" ON user_profiles;
DROP POLICY IF EXISTS "Enable read access for authenticated users" ON user_profiles;
DROP POLICY IF EXISTS "Enable write access for authenticated users" ON user_profiles;
DROP POLICY IF EXISTS "allow_authenticated_users" ON user_profiles;
DROP POLICY IF EXISTS "allow_all_user_profiles" ON user_profiles;
DROP POLICY IF EXISTS "Users can view their own profile" ON user_profiles;
DROP POLICY IF EXISTS "Admins can view all profiles" ON user_profiles;
DROP POLICY IF EXISTS "Users can update their own profile" ON user_profiles;
DROP POLICY IF EXISTS "Admins can manage all profiles" ON user_profiles;

-- 2. Create separate policies for each operation
-- Allow SELECT (read) for all authenticated users
CREATE POLICY "user_profiles_select_policy" ON user_profiles
FOR SELECT USING (auth.role() = 'authenticated');

-- Allow INSERT for all authenticated users
CREATE POLICY "user_profiles_insert_policy" ON user_profiles
FOR INSERT WITH CHECK (auth.role() = 'authenticated');

-- Allow UPDATE for all authenticated users
CREATE POLICY "user_profiles_update_policy" ON user_profiles
FOR UPDATE USING (auth.role() = 'authenticated')
WITH CHECK (auth.role() = 'authenticated');

-- Allow DELETE for all authenticated users
CREATE POLICY "user_profiles_delete_policy" ON user_profiles
FOR DELETE USING (auth.role() = 'authenticated');

-- 3. Test the policies
SELECT 'Testing user_profiles SELECT...' as test;
SELECT COUNT(*) as user_count FROM user_profiles;

-- Test specific user lookup (like login would do)
SELECT 'Testing email lookup...' as test;
SELECT id, email, full_name, role FROM user_profiles WHERE email = 'admin@campus360.com';

-- 4. Show current policies
SELECT 'Current user_profiles policies:' as info;
SELECT 
    policyname, 
    cmd as operation,
    CASE 
        WHEN cmd = 'SELECT' THEN 'Read Access'
        WHEN cmd = 'INSERT' THEN 'Create Access'
        WHEN cmd = 'UPDATE' THEN 'Update Access'
        WHEN cmd = 'DELETE' THEN 'Delete Access'
        ELSE cmd
    END as description
FROM pg_policies 
WHERE tablename = 'user_profiles'
ORDER BY cmd;

SELECT 'User profiles RLS fix completed successfully!' as status;
