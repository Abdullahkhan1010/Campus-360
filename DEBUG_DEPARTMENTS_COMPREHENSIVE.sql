-- ========================================
-- DEBUG DEPARTMENTS TABLE ACCESS
-- ========================================
-- Run this in Supabase SQL Editor to debug the departments issue

-- 1. Check if table exists and basic structure
SELECT 'Checking if departments table exists...' as step;
SELECT table_name, table_schema 
FROM information_schema.tables 
WHERE table_name = 'departments';

-- 2. Check column structure
SELECT 'Departments table structure:' as step;
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns 
WHERE table_name = 'departments'
ORDER BY ordinal_position;

-- 3. Count all records (without RLS)
SELECT 'Total departments count (bypassing RLS):' as step;
SET row_security = off;
SELECT COUNT(*) as total_departments FROM departments;
SET row_security = on;

-- 4. Check RLS status
SELECT 'RLS status for departments:' as step;
SELECT 
    schemaname,
    tablename,
    rowsecurity as rls_enabled,
    relname
FROM pg_class c
JOIN pg_namespace n ON c.relnamespace = n.oid
WHERE relname = 'departments' AND nspname = 'public';

-- 5. Check current policies
SELECT 'Current RLS policies on departments:' as step;
SELECT 
    policyname,
    permissive,
    roles,
    cmd,
    qual,
    with_check
FROM pg_policies 
WHERE tablename = 'departments';

-- 6. Test authenticated access (this simulates what the app does)
SELECT 'Testing authenticated access to departments:' as step;
SELECT COUNT(*) as accessible_departments FROM departments;

-- 7. Show sample records
SELECT 'Sample departments (first 5):' as step;
SELECT id, name, code, description, is_active 
FROM departments 
LIMIT 5;

-- 8. Check if there are any auth context issues
SELECT 'Current auth context:' as step;
SELECT 
    auth.uid() as current_user_id,
    auth.role() as current_role,
    auth.email() as current_email;

-- 9. Test direct table access with different approaches
SELECT 'Testing different access patterns:' as step;

-- Pattern 1: Direct SELECT *
SELECT 'Pattern 1 - SELECT *:' as test;
SELECT COUNT(*) FROM departments;

-- Pattern 2: Specific columns
SELECT 'Pattern 2 - Specific columns:' as test;
SELECT COUNT(*) FROM (SELECT id, name FROM departments) as test_query;

-- Pattern 3: With WHERE clause
SELECT 'Pattern 3 - With WHERE:' as test;
SELECT COUNT(*) FROM departments WHERE is_active = true;

SELECT 'Debugging completed!' as final_status;
