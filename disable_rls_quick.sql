-- TEMPORARY RLS DISABLE for testing
-- Run this in Supabase SQL Editor to quickly resolve RLS issues

-- Disable RLS on academic_events table
ALTER TABLE academic_events DISABLE ROW LEVEL SECURITY;

-- Disable RLS on file_documents table (if exists)
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'file_documents') THEN
        ALTER TABLE file_documents DISABLE ROW LEVEL SECURITY;
    END IF;
END
$$;

-- Disable RLS on user_profiles table
ALTER TABLE user_profiles DISABLE ROW LEVEL SECURITY;

-- Disable RLS on departments table
ALTER TABLE departments DISABLE ROW LEVEL SECURITY;

-- Disable RLS on courses table (if exists)
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'courses') THEN
        ALTER TABLE courses DISABLE ROW LEVEL SECURITY;
    END IF;
END
$$;

-- Disable RLS on student_enrollments table (if exists)
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'student_enrollments') THEN
        ALTER TABLE student_enrollments DISABLE ROW LEVEL SECURITY;
    END IF;
END
$$;

-- Verify tables and their RLS status
SELECT 
    schemaname,
    tablename, 
    rowsecurity as rls_enabled,
    -- Check if table has any RLS policies
    (SELECT count(*) FROM pg_policies WHERE schemaname = 'public' AND tablename = t.tablename) as policy_count
FROM pg_tables t 
WHERE schemaname = 'public' 
AND tablename IN ('academic_events', 'file_documents', 'user_profiles', 'departments', 'courses', 'student_enrollments')
ORDER BY tablename;
