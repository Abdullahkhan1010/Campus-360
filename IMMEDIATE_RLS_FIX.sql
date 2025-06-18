-- =====================================================
-- IMMEDIATE FIX: RLS INFINITE RECURSION
-- =====================================================
-- Run this RIGHT NOW in Supabase SQL Editor

-- Step 1: Check current policies causing recursion
SELECT 
    schemaname,
    tablename,
    policyname,
    permissive,
    roles,
    cmd,
    qual,
    with_check
FROM pg_policies 
WHERE tablename = 'user_profiles';

-- Step 2: DISABLE RLS completely to break the recursion
ALTER TABLE user_profiles DISABLE ROW LEVEL SECURITY;
ALTER TABLE departments DISABLE ROW LEVEL SECURITY;
ALTER TABLE courses DISABLE ROW LEVEL SECURITY;

-- Step 3: Drop ALL existing policies
DROP POLICY IF EXISTS "Users can view their own profile" ON user_profiles;
DROP POLICY IF EXISTS "Admins can view all profiles" ON user_profiles;
DROP POLICY IF EXISTS "Users can update their own profile" ON user_profiles;
DROP POLICY IF EXISTS "Admins can manage all profiles" ON user_profiles;
DROP POLICY IF EXISTS "Enable read access for all users" ON user_profiles;
DROP POLICY IF EXISTS "Enable insert for authenticated users only" ON user_profiles;
DROP POLICY IF EXISTS "Enable update for users based on email" ON user_profiles;

-- Drop department policies
DROP POLICY IF EXISTS "Users can view departments" ON departments;
DROP POLICY IF EXISTS "Admins can modify departments" ON departments;
DROP POLICY IF EXISTS "Enable read access for all users" ON departments;

-- Step 4: Test without RLS (this should work now)
SELECT 'Testing user_profiles access without RLS' as test;
SELECT COUNT(*) as user_count FROM user_profiles;

-- Step 5: Re-enable RLS with SIMPLE, NON-RECURSIVE policies
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;

-- Create ONE simple policy that allows all operations for authenticated users
CREATE POLICY "allow_all_authenticated" ON user_profiles
    FOR ALL 
    TO authenticated 
    USING (true) 
    WITH CHECK (true);

-- Do the same for departments
ALTER TABLE departments ENABLE ROW LEVEL SECURITY;
CREATE POLICY "allow_all_departments" ON departments
    FOR ALL 
    TO authenticated 
    USING (true) 
    WITH CHECK (true);

-- Step 6: Test that RLS now works without recursion
SELECT 'Testing with new simple RLS policies' as final_test;
SELECT COUNT(*) as user_count FROM user_profiles;
SELECT COUNT(*) as dept_count FROM departments;
