-- Check user_profiles table constraints and triggers
-- Run this in Supabase SQL Editor

-- Check table structure and constraints
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default,
    constraint_name,
    constraint_type
FROM information_schema.columns c
LEFT JOIN information_schema.constraint_column_usage ccu ON c.column_name = ccu.column_name AND c.table_name = ccu.table_name
LEFT JOIN information_schema.table_constraints tc ON ccu.constraint_name = tc.constraint_name
WHERE c.table_name = 'user_profiles'
ORDER BY c.ordinal_position;

-- Check if there are any triggers on user_profiles
SELECT 
    trigger_name,
    event_manipulation,
    action_timing,
    action_statement
FROM information_schema.triggers 
WHERE event_object_table = 'user_profiles';

-- Check RLS status
SELECT 
    schemaname, 
    tablename, 
    rowsecurity as rls_enabled
FROM pg_tables 
WHERE tablename = 'user_profiles';

-- Check existing policies
SELECT 
    schemaname, 
    tablename, 
    policyname,
    cmd,
    permissive
FROM pg_policies 
WHERE tablename = 'user_profiles';

-- Test if we can manually insert a user (this should work)
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
    gen_random_uuid()::text,
    'test-manual@campus360.com',
    'Test Manual User',
    'student',
    (SELECT id FROM departments WHERE is_active = true LIMIT 1),
    true,
    true,
    NOW(),
    NOW()
) ON CONFLICT (email) DO NOTHING;

-- Check if the manual insert worked
SELECT id, email, full_name, role FROM user_profiles WHERE email = 'test-manual@campus360.com';
