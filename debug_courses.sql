-- Debug script to check course table and data
-- Run this in Supabase SQL Editor

-- Check if courses table exists and its structure
SELECT table_name, column_name, data_type, is_nullable
FROM information_schema.columns 
WHERE table_name = 'courses' 
ORDER BY ordinal_position;

-- Check RLS status
SELECT 
    schemaname, 
    tablename, 
    rowsecurity as rls_enabled
FROM pg_tables 
WHERE tablename = 'courses';

-- Check existing courses
SELECT id, name, code, department_id, teacher_id, is_active, created_at
FROM courses 
ORDER BY created_at DESC;

-- Check departments (needed for course creation)
SELECT id, name, code, is_active 
FROM departments 
ORDER BY name;

-- Check if there are any RLS policies on courses
SELECT 
    schemaname, 
    tablename, 
    policyname,
    cmd,
    permissive
FROM pg_policies 
WHERE tablename = 'courses';

-- Test inserting a sample course
INSERT INTO courses (
    id, 
    name, 
    code, 
    description, 
    department_id, 
    semester, 
    credit_hours, 
    is_active, 
    created_at, 
    updated_at
) VALUES (
    gen_random_uuid(),
    'Test Course',
    'TEST101',
    'A test course for debugging',
    (SELECT id FROM departments WHERE is_active = true LIMIT 1),
    1,
    3,
    true,
    NOW(),
    NOW()
) ON CONFLICT (code) DO NOTHING;

-- Verify the test course was inserted
SELECT COUNT(*) as total_courses FROM courses;
SELECT name, code, department_id FROM courses WHERE code = 'TEST101';
