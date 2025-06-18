-- Create a simple test admin user directly in the database
-- Run this in Supabase SQL Editor after confirming the table exists

-- First, let's see what's in the auth.users table
SELECT id, email, created_at FROM auth.users WHERE email = 'admin@campus360.com';

-- If the auth user exists but no profile, insert the profile
-- Replace the UUID below with the actual UUID from the auth.users query above
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
ON CONFLICT (email) DO UPDATE SET
    full_name = EXCLUDED.full_name,
    role = EXCLUDED.role,
    is_verified = EXCLUDED.is_verified,
    updated_at = NOW();

-- Verify the profile was created
SELECT * FROM user_profiles WHERE email = 'admin@campus360.com';
