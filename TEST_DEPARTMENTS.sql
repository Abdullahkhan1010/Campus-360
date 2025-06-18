-- Test departments table access
-- Run this in Supabase SQL Editor to check if departments are accessible

-- 1. Check if departments table exists and has data
SELECT COUNT(*) as department_count FROM departments;

-- 2. Check department structure
SELECT * FROM departments LIMIT 5;

-- 3. Check RLS policies on departments
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
WHERE tablename = 'departments';

-- 4. Check if RLS is enabled
SELECT relname, relrowsecurity 
FROM pg_class 
WHERE relname = 'departments';

-- 5. Test direct access to departments
SET row_security = off;
SELECT * FROM departments;
SET row_security = on;
