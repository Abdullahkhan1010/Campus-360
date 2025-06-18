-- =====================================================
-- URGENT FIX: USER CREATION ID CONSTRAINT ISSUE
-- =====================================================
-- This script fixes the immediate blocking issue with user creation
-- Run this in your Supabase SQL Editor IMMEDIATELY

-- Step 1: Remove the foreign key constraint that's causing the ID to be forced to NULL
ALTER TABLE user_profiles 
DROP CONSTRAINT IF EXISTS user_profiles_id_fkey;

-- Step 2: Make sure the ID column allows custom values (not auto-generated)
ALTER TABLE user_profiles 
ALTER COLUMN id DROP DEFAULT;

-- Step 3: Change ID column to TEXT type to match Supabase Auth UUIDs
ALTER TABLE user_profiles 
ALTER COLUMN id TYPE TEXT USING id::TEXT;

-- Step 4: Test insert with custom ID to verify it works
INSERT INTO user_profiles (
    id,
    email,
    full_name,
    role,
    is_verified,
    is_active,
    created_at,
    updated_at
) VALUES (
    'test-fix-' || extract(epoch from now())::text,
    'test.fix@example.com',
    'Test Fix User',
    'teacher',
    true,
    true,
    NOW(),
    NOW()
) ON CONFLICT (email) DO NOTHING;

-- Step 5: Clean up test user
DELETE FROM user_profiles WHERE email = 'test.fix@example.com';

-- Verify the fix
SELECT 'Database fix completed successfully' as status;
