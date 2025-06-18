-- Temporary fix: Disable RLS for testing and create admin user
-- Run this in Supabase SQL Editor

-- Temporarily disable RLS for user_profiles to avoid recursion
ALTER TABLE user_profiles DISABLE ROW LEVEL SECURITY;

-- Also disable RLS on departments and other core tables
ALTER TABLE departments DISABLE ROW LEVEL SECURITY;
ALTER TABLE courses DISABLE ROW LEVEL SECURITY;
ALTER TABLE course_enrollments DISABLE ROW LEVEL SECURITY;
ALTER TABLE assignments DISABLE ROW LEVEL SECURITY;
ALTER TABLE assignment_submissions DISABLE ROW LEVEL SECURITY;
ALTER TABLE attendance_sessions DISABLE ROW LEVEL SECURITY;
ALTER TABLE attendance_records DISABLE ROW LEVEL SECURITY;
ALTER TABLE notifications DISABLE ROW LEVEL SECURITY;
ALTER TABLE activity_logs DISABLE ROW LEVEL SECURITY;
ALTER TABLE file_documents DISABLE ROW LEVEL SECURITY;

-- CRITICAL: Disable RLS on academic_events table
ALTER TABLE academic_events DISABLE ROW LEVEL SECURITY;

-- Create or update the admin profile if it doesn't exist
INSERT INTO user_profiles (
    id,
    email,
    full_name,
    first_name,
    last_name,
    role,
    department_id,
    is_verified,
    is_active,
    created_at,
    updated_at
) 
SELECT 
    auth.id,
    'admin@campus360.com',
    'Campus Administrator',
    'Campus',
    'Administrator',
    'admin',
    (SELECT id FROM departments WHERE code = 'ADMIN' LIMIT 1),
    true,
    true,
    NOW(),
    NOW()
FROM auth.users auth
WHERE auth.email = 'admin@campus360.com'
  AND NOT EXISTS (SELECT 1 FROM user_profiles WHERE email = 'admin@campus360.com')
ON CONFLICT (email) DO UPDATE SET
    full_name = EXCLUDED.full_name,
    role = EXCLUDED.role,
    is_verified = EXCLUDED.is_verified,
    updated_at = NOW();

-- Verify the admin profile exists
SELECT id, email, full_name, role, is_verified, is_active 
FROM user_profiles 
WHERE email = 'admin@campus360.com';

-- Create admin department if it doesn't exist
INSERT INTO departments (id, name, code, description, is_active, created_at, updated_at)
VALUES (
    gen_random_uuid(),
    'Administration',
    'ADMIN',
    'Administrative Department',
    true,
    NOW(),
    NOW()
) ON CONFLICT (code) DO NOTHING;

-- Check RLS status
SELECT 
    schemaname, 
    tablename, 
    rowsecurity as rls_enabled
FROM pg_tables 
WHERE schemaname = 'public' 
  AND tablename IN ('departments', 'courses', 'user_profiles', 'course_enrollments', 'assignments')
ORDER BY tablename;

-- For now, keep RLS disabled until we fix the policies
-- We'll re-enable it later with proper policies

SELECT 'RLS temporarily disabled for user_profiles table. Admin user should now work.' as status;
SELECT 'All main tables now have RLS disabled for development. Re-enable with proper policies before production!' as warning;
