-- =====================================================
-- FIX USER CREATION - REMOVE FOREIGN KEY CONSTRAINT
-- =====================================================
-- This script removes the foreign key constraint that prevents
-- independent user profile creation without auth records
-- and changes the ID column to TEXT to work with Supabase Auth

-- Step 1: Remove the foreign key constraint from user_profiles.id
ALTER TABLE user_profiles 
DROP CONSTRAINT IF EXISTS user_profiles_id_fkey;

-- Step 2: Change the ID column from UUID to TEXT to work with Supabase Auth
ALTER TABLE user_profiles 
ALTER COLUMN id TYPE TEXT USING id::TEXT;

-- Step 3: Update any foreign key references to use TEXT
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

-- Step 4: Verify the constraint is removed and column is changed
SELECT 
    'Foreign key constraint removed and ID column changed to TEXT' as status,
    COUNT(*) as remaining_foreign_keys
FROM information_schema.table_constraints 
WHERE constraint_type = 'FOREIGN KEY' 
    AND table_name = 'user_profiles'
    AND constraint_name LIKE '%_id_fkey';

-- Step 5: Test inserting a user profile with string ID (like Supabase Auth)
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
    'test.user.constraint@example.com',
    'Test User Constraint',
    'teacher',
    (SELECT id FROM departments WHERE is_active = true LIMIT 1),
    true,
    true,
    NOW(),
    NOW()
) ON CONFLICT (email) DO NOTHING;

-- Step 6: Verify the test user was inserted
SELECT 
    id, 
    email, 
    full_name, 
    role,
    'Test user created successfully - constraint fix complete' as status
FROM user_profiles 
WHERE email = 'test.user.constraint@example.com';

-- Step 7: Clean up test user (optional)
-- DELETE FROM user_profiles WHERE email = 'test.user.constraint@example.com';
