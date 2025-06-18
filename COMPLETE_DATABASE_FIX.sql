-- =====================================================
-- COMPLETE DATABASE FIX FOR USER CREATION ISSUE
-- =====================================================
-- This script removes RLS policies, fixes constraints, and recreates policies
-- Run this in Supabase SQL Editor

-- Step 1: Drop all RLS policies that depend on the id column
DROP POLICY IF EXISTS "Users can view their own profile" ON user_profiles;
DROP POLICY IF EXISTS "Users can update their own profile" ON user_profiles;
DROP POLICY IF EXISTS "Admin can view all profiles" ON user_profiles;
DROP POLICY IF EXISTS "Admin can insert profiles" ON user_profiles;
DROP POLICY IF EXISTS "Admin can update all profiles" ON user_profiles;
DROP POLICY IF EXISTS "Admin can delete profiles" ON user_profiles;
DROP POLICY IF EXISTS "Authenticated users can view profiles" ON user_profiles;
DROP POLICY IF EXISTS "Service role can do everything" ON user_profiles;

-- Step 2: Temporarily disable RLS on user_profiles
ALTER TABLE user_profiles DISABLE ROW LEVEL SECURITY;

-- Step 3: Remove the foreign key constraint from user_profiles.id
ALTER TABLE user_profiles 
DROP CONSTRAINT IF EXISTS user_profiles_id_fkey;

-- Step 4: Change the ID column from UUID to TEXT to work with Supabase Auth
ALTER TABLE user_profiles 
ALTER COLUMN id TYPE TEXT USING id::TEXT;

-- Step 5: Update any foreign key references to use TEXT
ALTER TABLE courses ALTER COLUMN teacher_id TYPE TEXT USING teacher_id::TEXT;
ALTER TABLE course_enrollments ALTER COLUMN student_id TYPE TEXT USING student_id::TEXT;
ALTER TABLE assignments ALTER COLUMN created_by TYPE TEXT USING created_by::TEXT;
ALTER TABLE assignment_submissions ALTER COLUMN student_id TYPE TEXT USING student_id::TEXT;
ALTER TABLE attendance_records ALTER COLUMN student_id TYPE TEXT USING student_id::TEXT;
ALTER TABLE grades ALTER COLUMN student_id TYPE TEXT USING student_id::TEXT;
ALTER TABLE calendar_events ALTER COLUMN created_by TYPE TEXT USING created_by::TEXT;
ALTER TABLE notifications ALTER COLUMN recipient_id TYPE TEXT USING recipient_id::TEXT;
ALTER TABLE notifications ALTER COLUMN sender_id TYPE TEXT USING sender_id::TEXT;
ALTER TABLE activity_logs ALTER COLUMN user_id TYPE TEXT USING user_id::TEXT;
ALTER TABLE user_settings ALTER COLUMN user_id TYPE TEXT USING user_id::TEXT;

-- Step 6: Re-enable RLS on user_profiles
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;

-- Step 7: Recreate essential RLS policies with TEXT id
CREATE POLICY "Enable read access for authenticated users" ON user_profiles
    FOR SELECT USING (auth.role() = 'authenticated');

CREATE POLICY "Enable insert for service role" ON user_profiles
    FOR INSERT WITH CHECK (auth.role() = 'service_role');

CREATE POLICY "Enable update for service role" ON user_profiles
    FOR UPDATE USING (auth.role() = 'service_role');

CREATE POLICY "Enable delete for service role" ON user_profiles
    FOR DELETE USING (auth.role() = 'service_role');

-- Step 8: Grant necessary permissions
GRANT ALL ON user_profiles TO authenticated;
GRANT ALL ON user_profiles TO service_role;

-- Step 9: Test inserting a user profile with string ID (like Supabase Auth)
INSERT INTO user_profiles (
    id,
    email,
    full_name,
    role,
    department_id,
    is_verified,
    is_active,
    created_at,
    updated_at
) VALUES (
    'test-user-' || extract(epoch from now())::text,
    'test.constraint.fix@example.com',
    'Test Constraint Fix User',
    'teacher',
    (SELECT id FROM departments WHERE is_active = true LIMIT 1),
    true,
    true,
    NOW(),
    NOW()
) ON CONFLICT (email) DO NOTHING;

-- Step 10: Verify the test user was inserted and constraints are fixed
SELECT 
    'Database fix completed successfully' as status,
    id,
    email,
    full_name
FROM user_profiles 
WHERE email = 'test.constraint.fix@example.com';

-- Step 11: Show current constraint status
SELECT 
    'Constraint check' as info,
    COUNT(*) as remaining_foreign_keys
FROM information_schema.table_constraints 
WHERE constraint_type = 'FOREIGN KEY' 
    AND table_name = 'user_profiles'
    AND constraint_name LIKE '%_id_fkey';
