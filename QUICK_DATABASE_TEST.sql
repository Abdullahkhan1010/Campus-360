-- =====================================================
-- QUICK DATABASE TEST FOR USER CREATION
-- =====================================================
-- This script tests if user creation works after fixes
-- Run this before the full fix script to check current state

-- Test 1: Check current ID column type
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'user_profiles' 
    AND column_name = 'id';

-- Test 2: Check existing constraints
SELECT 
    constraint_name,
    constraint_type,
    table_name
FROM information_schema.table_constraints 
WHERE table_name = 'user_profiles' 
    AND constraint_type = 'FOREIGN KEY';

-- Test 3: Try a simple insert with text ID
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
    'test-manual-' || extract(epoch from now())::text,
    'manual.test@example.com',
    'Manual Test User',
    'student',
    true,
    true,
    NOW(),
    NOW()
) ON CONFLICT (email) DO NOTHING;

-- Test 4: Check if the test user was created
SELECT 
    id,
    email,
    full_name,
    role,
    created_at
FROM user_profiles 
WHERE email = 'manual.test@example.com';

-- Test 5: Clean up test user
DELETE FROM user_profiles WHERE email = 'manual.test@example.com';

SELECT 'Database test completed' as status;
