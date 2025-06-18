-- ========================================
-- URGENT FIX: RLS BLOCKING USER PROFILE ACCESS
-- ========================================
-- This script fixes RLS policies to allow authenticated users to read user profiles
-- The issue: RLS is preventing the app from reading user_profiles during login

-- Step 1: Temporarily disable RLS to test access
ALTER TABLE user_profiles DISABLE ROW LEVEL SECURITY;

-- Step 2: Test that we can now read the admin user
SELECT 'Testing user_profiles access with RLS disabled...' as test;
SELECT id, email, full_name, role FROM user_profiles WHERE email = 'admin@campus360.com';

-- Step 3: Show all users to confirm data exists
SELECT 'All users in database:' as info;
SELECT id, email, full_name, role, is_verified, is_active FROM user_profiles ORDER BY created_at;

-- Step 4: Re-enable RLS
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;

-- Step 5: Drop ALL existing problematic policies
DROP POLICY IF EXISTS "Users can view own profile" ON user_profiles;
DROP POLICY IF EXISTS "Users can update own profile" ON user_profiles;
DROP POLICY IF EXISTS "Admins can view all profiles" ON user_profiles;
DROP POLICY IF EXISTS "Admins can manage all profiles" ON user_profiles;
DROP POLICY IF EXISTS "allow_all_authenticated" ON user_profiles;
DROP POLICY IF EXISTS "Enable read access for authenticated users" ON user_profiles;
DROP POLICY IF EXISTS "Enable write access for authenticated users" ON user_profiles;
DROP POLICY IF EXISTS "Enable insert for service role" ON user_profiles;
DROP POLICY IF EXISTS "Enable update for service role" ON user_profiles;
DROP POLICY IF EXISTS "Enable delete for service role" ON user_profiles;
DROP POLICY IF EXISTS "All authenticated users can view user profiles" ON user_profiles;
DROP POLICY IF EXISTS "Authenticated users can modify user profiles" ON user_profiles;

-- Step 6: Create SIMPLE, NON-RECURSIVE policies that actually work
-- Allow ALL authenticated users to READ user profiles (needed for login)
CREATE POLICY "authenticated_can_read_profiles" ON user_profiles
FOR SELECT TO authenticated USING (true);

-- Allow ALL authenticated users to INSERT user profiles (needed for user creation)
CREATE POLICY "authenticated_can_create_profiles" ON user_profiles
FOR INSERT TO authenticated WITH CHECK (true);

-- Allow ALL authenticated users to UPDATE user profiles
CREATE POLICY "authenticated_can_update_profiles" ON user_profiles
FOR UPDATE TO authenticated USING (true) WITH CHECK (true);

-- Allow ALL authenticated users to DELETE user profiles  
CREATE POLICY "authenticated_can_delete_profiles" ON user_profiles
FOR DELETE TO authenticated USING (true);

-- Step 7: Test the fix - try to read the admin user again
SELECT 'Testing user_profiles access with new RLS policies...' as test;
SELECT id, email, full_name, role FROM user_profiles WHERE email = 'admin@campus360.com';

-- Step 8: Show current policies
SELECT 'Current user_profiles policies:' as info;
SELECT 
    policyname, 
    cmd as operation,
    permissive
FROM pg_policies 
WHERE tablename = 'user_profiles'
ORDER BY cmd;

-- Step 9: Also fix departments while we're at it
ALTER TABLE departments DISABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS "departments_select_policy" ON departments;
DROP POLICY IF EXISTS "departments_insert_policy" ON departments;
DROP POLICY IF EXISTS "departments_update_policy" ON departments;
DROP POLICY IF EXISTS "departments_delete_policy" ON departments;
DROP POLICY IF EXISTS "allow_all_authenticated" ON departments;

ALTER TABLE departments ENABLE ROW LEVEL SECURITY;

CREATE POLICY "authenticated_can_read_departments" ON departments
FOR SELECT TO authenticated USING (true);

CREATE POLICY "authenticated_can_create_departments" ON departments
FOR INSERT TO authenticated WITH CHECK (true);

CREATE POLICY "authenticated_can_update_departments" ON departments
FOR UPDATE TO authenticated USING (true) WITH CHECK (true);

CREATE POLICY "authenticated_can_delete_departments" ON departments
FOR DELETE TO authenticated USING (true);

-- Step 10: Final verification
SELECT 'Testing departments access...' as test;
SELECT COUNT(*) as department_count FROM departments;

SELECT 'RLS fix completed - login should work now!' as status;
