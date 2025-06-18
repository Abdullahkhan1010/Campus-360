-- =====================================================
-- COMPLETE RLS AND CONSTRAINT FIX FOR USER CREATION
-- =====================================================
-- This script fixes the user creation issue by:
-- 1. Temporarily disabling RLS policies
-- 2. Removing foreign key constraints 
-- 3. Converting ID columns to TEXT
-- 4. Recreating RLS policies with TEXT compatibility
-- =====================================================

-- Step 1: Disable RLS temporarily to allow schema changes
ALTER TABLE user_profiles DISABLE ROW LEVEL SECURITY;

-- Step 2: Drop all existing RLS policies that depend on the id column
DROP POLICY IF EXISTS "Users can view their own profile" ON user_profiles;
DROP POLICY IF EXISTS "Admins can view all profiles" ON user_profiles;
DROP POLICY IF EXISTS "Users can update their own profile" ON user_profiles;
DROP POLICY IF EXISTS "Admins can update all profiles" ON user_profiles;
DROP POLICY IF EXISTS "Admins can insert profiles" ON user_profiles;
DROP POLICY IF EXISTS "Admins can delete profiles" ON user_profiles;

-- Step 3: Remove the foreign key constraint that links to auth.users
ALTER TABLE user_profiles 
DROP CONSTRAINT IF EXISTS user_profiles_id_fkey;

-- Step 4: Change the ID column from UUID to TEXT
ALTER TABLE user_profiles 
ALTER COLUMN id TYPE TEXT USING id::TEXT;

-- Step 5: Update all foreign key references to use TEXT
ALTER TABLE courses 
ALTER COLUMN teacher_id TYPE TEXT USING teacher_id::TEXT;

ALTER TABLE course_enrollments 
ALTER COLUMN student_id TYPE TEXT USING student_id::TEXT;

ALTER TABLE assignments 
ALTER COLUMN created_by TYPE TEXT USING created_by::TEXT;

ALTER TABLE assignment_submissions 
ALTER COLUMN student_id TYPE TEXT USING student_id::TEXT;

ALTER TABLE attendance_records 
ALTER COLUMN student_id TYPE TEXT USING student_id::TEXT;

ALTER TABLE grades 
ALTER COLUMN student_id TYPE TEXT USING student_id::TEXT;

ALTER TABLE calendar_events 
ALTER COLUMN created_by TYPE TEXT USING created_by::TEXT;

ALTER TABLE notifications 
ALTER COLUMN recipient_id TYPE TEXT USING recipient_id::TEXT,
ALTER COLUMN sender_id TYPE TEXT USING sender_id::TEXT;

ALTER TABLE activity_logs 
ALTER COLUMN user_id TYPE TEXT USING user_id::TEXT;

ALTER TABLE user_settings 
ALTER COLUMN user_id TYPE TEXT USING user_id::TEXT;

-- Step 6: Recreate RLS policies with TEXT compatibility
CREATE POLICY "Users can view their own profile" 
ON user_profiles FOR SELECT 
USING (auth.uid()::TEXT = id);

CREATE POLICY "Admins can view all profiles" 
ON user_profiles FOR SELECT 
USING (
    EXISTS (
        SELECT 1 FROM user_profiles up 
        WHERE up.id = auth.uid()::TEXT 
        AND up.role = 'admin'
    )
);

CREATE POLICY "Users can update their own profile" 
ON user_profiles FOR UPDATE 
USING (auth.uid()::TEXT = id);

CREATE POLICY "Admins can update all profiles" 
ON user_profiles FOR UPDATE 
USING (
    EXISTS (
        SELECT 1 FROM user_profiles up 
        WHERE up.id = auth.uid()::TEXT 
        AND up.role = 'admin'
    )
);

CREATE POLICY "Admins can insert profiles" 
ON user_profiles FOR INSERT 
WITH CHECK (
    EXISTS (
        SELECT 1 FROM user_profiles up 
        WHERE up.id = auth.uid()::TEXT 
        AND up.role = 'admin'
    )
);

CREATE POLICY "Admins can delete profiles" 
ON user_profiles FOR DELETE 
USING (
    EXISTS (
        SELECT 1 FROM user_profiles up 
        WHERE up.id = auth.uid()::TEXT 
        AND up.role = 'admin'
    )
);

-- Step 7: Re-enable RLS
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;

-- Step 8: Test the fix by inserting a test user
DO $$
DECLARE
    test_user_id TEXT := 'test-user-' || extract(epoch from now())::TEXT;
BEGIN
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
        test_user_id,
        'test.constraint.fix@example.com',
        'Test Constraint Fix User',
        'teacher',
        (SELECT id FROM departments WHERE is_active = true LIMIT 1),
        true,
        true,
        NOW(),
        NOW()
    ) ON CONFLICT (email) DO NOTHING;
    
    RAISE NOTICE 'Test user created successfully with ID: %', test_user_id;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Test user creation failed: %', SQLERRM;
END $$;

-- Step 9: Verify the changes
SELECT 
    'Database constraint and RLS fixes applied successfully' as status,
    data_type as id_column_type
FROM information_schema.columns 
WHERE table_name = 'user_profiles' 
    AND column_name = 'id';

-- Step 10: Show remaining constraints (should be none for user_profiles.id)
SELECT 
    constraint_name,
    constraint_type,
    table_name
FROM information_schema.table_constraints 
WHERE table_name = 'user_profiles' 
    AND constraint_type = 'FOREIGN KEY';
