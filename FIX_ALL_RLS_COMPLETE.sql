-- ========================================
-- COMPLETE RLS FIX: User Profiles + Departments
-- ========================================
-- This script fixes RLS issues for both user_profiles and departments tables
-- Copy and paste this ENTIRE script into Supabase SQL Editor and run it

-- ========================================
-- 1. FIX USER_PROFILES RLS
-- ========================================

-- Disable RLS on user_profiles
ALTER TABLE user_profiles DISABLE ROW LEVEL SECURITY;

-- Drop ALL existing policies that might cause recursion
DROP POLICY IF EXISTS "Users can view their own profile" ON user_profiles;
DROP POLICY IF EXISTS "Admins can view all profiles" ON user_profiles;
DROP POLICY IF EXISTS "Users can update their own profile" ON user_profiles;
DROP POLICY IF EXISTS "Admins can manage all profiles" ON user_profiles;
DROP POLICY IF EXISTS "allow_authenticated_users" ON user_profiles;
DROP POLICY IF EXISTS "allow_all_user_profiles" ON user_profiles;
DROP POLICY IF EXISTS "Enable read access for authenticated users" ON user_profiles;
DROP POLICY IF EXISTS "Enable write access for authenticated users" ON user_profiles;

-- Re-enable RLS
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;

-- Create simple, non-recursive policy
CREATE POLICY "allow_all_authenticated" ON user_profiles
FOR ALL USING (auth.role() = 'authenticated');

-- ========================================
-- 2. FIX DEPARTMENTS RLS
-- ========================================

-- Disable RLS on departments
ALTER TABLE departments DISABLE ROW LEVEL SECURITY;

-- Drop ALL existing policies on departments
DROP POLICY IF EXISTS "Enable read access for authenticated users" ON departments;
DROP POLICY IF EXISTS "Enable write access for authenticated users" ON departments;
DROP POLICY IF EXISTS "allow_authenticated_users" ON departments;
DROP POLICY IF EXISTS "allow_all_departments" ON departments;
DROP POLICY IF EXISTS "Admins can manage departments" ON departments;
DROP POLICY IF EXISTS "Users can view departments" ON departments;

-- Re-enable RLS
ALTER TABLE departments ENABLE ROW LEVEL SECURITY;

-- Create simple, permissive policy for departments
CREATE POLICY "allow_all_authenticated" ON departments
FOR ALL USING (auth.role() = 'authenticated');

-- ========================================
-- 3. TEST ACCESS
-- ========================================

-- Test user_profiles access
SELECT 'Testing user_profiles access...' as test_name;
SELECT COUNT(*) as user_count FROM user_profiles;

-- Test departments access
SELECT 'Testing departments access...' as test_name;
SELECT COUNT(*) as department_count FROM departments;

-- Show first few records if they exist
SELECT 'Sample user_profiles records:' as info;
SELECT id, email, full_name, role FROM user_profiles LIMIT 3;

SELECT 'Sample departments records:' as info;
SELECT id, name, description, is_active FROM departments LIMIT 5;

-- ========================================
-- 4. VERIFY POLICIES
-- ========================================

-- Show current policies
SELECT 'Current user_profiles policies:' as info;
SELECT policyname, cmd, qual FROM pg_policies WHERE tablename = 'user_profiles';

SELECT 'Current departments policies:' as info;
SELECT policyname, cmd, qual FROM pg_policies WHERE tablename = 'departments';

SELECT 'RLS Fix completed successfully!' as status;
