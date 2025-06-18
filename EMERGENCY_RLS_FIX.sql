-- ========================================
-- EMERGENCY RLS FIX - INFINITE RECURSION
-- ========================================
-- This script fixes the infinite recursion in RLS policies

-- 1. DISABLE RLS COMPLETELY
ALTER TABLE user_profiles DISABLE ROW LEVEL SECURITY;
ALTER TABLE departments DISABLE ROW LEVEL SECURITY;

-- 2. DROP ALL EXISTING POLICIES
DROP POLICY IF EXISTS "allow_everything" ON user_profiles;
DROP POLICY IF EXISTS "allow_everything" ON departments;
DROP POLICY IF EXISTS "user_profiles_select_policy" ON user_profiles;
DROP POLICY IF EXISTS "user_profiles_insert_policy" ON user_profiles;
DROP POLICY IF EXISTS "user_profiles_update_policy" ON user_profiles;
DROP POLICY IF EXISTS "user_profiles_delete_policy" ON user_profiles;
DROP POLICY IF EXISTS "departments_select_policy" ON departments;
DROP POLICY IF EXISTS "departments_insert_policy" ON departments;
DROP POLICY IF EXISTS "departments_update_policy" ON departments;
DROP POLICY IF EXISTS "departments_delete_policy" ON departments;

-- 3. TEST ACCESS WITHOUT RLS
SELECT 'Testing without RLS:' as info;
SELECT COUNT(*) as user_count FROM user_profiles;
SELECT COUNT(*) as dept_count FROM departments;

-- 4. RE-ENABLE RLS
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE departments ENABLE ROW LEVEL SECURITY;

-- 5. CREATE SIMPLE, NON-RECURSIVE POLICIES
-- For user_profiles - allow access for authenticated users
CREATE POLICY "select_for_authenticated" ON user_profiles
FOR SELECT TO authenticated USING (true);

CREATE POLICY "insert_for_authenticated" ON user_profiles
FOR INSERT TO authenticated WITH CHECK (true);

CREATE POLICY "update_for_authenticated" ON user_profiles
FOR UPDATE TO authenticated USING (true) WITH CHECK (true);

CREATE POLICY "delete_for_authenticated" ON user_profiles
FOR DELETE TO authenticated USING (true);

-- For departments - allow access for authenticated users
CREATE POLICY "select_for_authenticated" ON departments
FOR SELECT TO authenticated USING (true);

CREATE POLICY "insert_for_authenticated" ON departments
FOR INSERT TO authenticated WITH CHECK (true);

CREATE POLICY "update_for_authenticated" ON departments
FOR UPDATE TO authenticated USING (true) WITH CHECK (true);

CREATE POLICY "delete_for_authenticated" ON departments
FOR DELETE TO authenticated USING (true);

-- 6. TEST ACCESS WITH NEW POLICIES
SELECT 'Testing with new policies:' as info;
SELECT COUNT(*) as user_count FROM user_profiles;
SELECT COUNT(*) as dept_count FROM departments;

-- 7. VERIFY ADMIN USER
SELECT 'Admin user verification:' as info;
SELECT id, email, full_name, role FROM user_profiles WHERE email = 'admin@campus360.com';

-- 8. SHOW CURRENT POLICIES
SELECT 'Current policies:' as info;
SELECT tablename, policyname, cmd FROM pg_policies 
WHERE tablename IN ('user_profiles', 'departments') 
ORDER BY tablename, cmd;

SELECT 'RLS infinite recursion fixed!' as status;
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;
CREATE POLICY "full_access" ON user_profiles FOR ALL USING (true) WITH CHECK (true);

-- 5. Final test
SELECT 'Final test after creating simple policy...' as status;
SELECT COUNT(*) as final_user_count FROM user_profiles;

-- 6. Success message
SELECT 'SUCCESS: RLS infinite recursion fixed!' as final_status;
