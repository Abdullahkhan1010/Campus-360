-- Temporary fix for user_profiles table to allow independent user creation
-- Run this in Supabase SQL Editor
-- WARNING: This modifies the constraint - use only for development!

-- Drop the foreign key constraint temporarily
ALTER TABLE user_profiles DROP CONSTRAINT IF EXISTS user_profiles_id_fkey;

-- Make the ID field independent (not referencing auth.users)
-- The ID will still be UUID PRIMARY KEY but won't require auth user
-- Note: This means these users won't be able to authenticate until we create auth records

-- Verify the constraint is removed
SELECT 
    tc.constraint_name,
    tc.table_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name 
FROM information_schema.table_constraints AS tc 
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
    AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY' 
    AND tc.table_name='user_profiles';

-- Test inserting a user profile
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
    gen_random_uuid(),
    'test.user@campus360.com',
    'Test User',
    'teacher',
    (SELECT id FROM departments WHERE is_active = true LIMIT 1),
    true,
    true,
    NOW(),
    NOW()
) ON CONFLICT (email) DO NOTHING;

-- Verify the test user was inserted
SELECT id, email, full_name, role FROM user_profiles WHERE email = 'test.user@campus360.com';

SELECT 'Foreign key constraint removed. User profiles can now be created independently.' as status;
