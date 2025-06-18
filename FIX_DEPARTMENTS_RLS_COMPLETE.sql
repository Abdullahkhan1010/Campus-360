-- ========================================
-- FIX DEPARTMENTS RLS POLICIES FOR ALL OPERATIONS
-- ========================================
-- This script fixes RLS policies to allow SELECT, INSERT, UPDATE, DELETE on departments

-- 1. Drop all existing policies on departments
DROP POLICY IF EXISTS "allow_all_authenticated" ON departments;
DROP POLICY IF EXISTS "Enable read access for authenticated users" ON departments;
DROP POLICY IF EXISTS "Enable write access for authenticated users" ON departments;
DROP POLICY IF EXISTS "allow_authenticated_users" ON departments;
DROP POLICY IF EXISTS "allow_all_departments" ON departments;
DROP POLICY IF EXISTS "Admins can manage departments" ON departments;
DROP POLICY IF EXISTS "Users can view departments" ON departments;

-- 2. Create separate policies for each operation
-- Allow SELECT (read) for all authenticated users
CREATE POLICY "departments_select_policy" ON departments
FOR SELECT USING (auth.role() = 'authenticated');

-- Allow INSERT for all authenticated users
CREATE POLICY "departments_insert_policy" ON departments
FOR INSERT WITH CHECK (auth.role() = 'authenticated');

-- Allow UPDATE for all authenticated users
CREATE POLICY "departments_update_policy" ON departments
FOR UPDATE USING (auth.role() = 'authenticated')
WITH CHECK (auth.role() = 'authenticated');

-- Allow DELETE for all authenticated users
CREATE POLICY "departments_delete_policy" ON departments
FOR DELETE USING (auth.role() = 'authenticated');

-- 3. Test the policies
SELECT 'Testing departments SELECT...' as test;
SELECT COUNT(*) as department_count FROM departments;

-- Test INSERT by trying to insert a test record (will be rolled back)
BEGIN;
INSERT INTO departments (id, name, code, description, is_active, created_at, updated_at)
VALUES ('test_dept', 'Test Department', 'TEST', 'Test Description', true, NOW(), NOW());
SELECT 'INSERT test successful' as result;
ROLLBACK;

-- 4. Show current policies
SELECT 'Current departments policies:' as info;
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
WHERE tablename = 'departments'
ORDER BY cmd;

SELECT 'Departments RLS fix completed successfully!' as status;
