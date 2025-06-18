-- =====================================================
-- CREATE ADMIN USER FOR TESTING
-- =====================================================
-- Run this AFTER fixing the RLS policies

-- First, check if admin user exists
SELECT 
    'Checking for existing admin user' as status,
    COUNT(*) as admin_count 
FROM user_profiles 
WHERE email = 'admin@campus360.com';

-- Create admin user if it doesn't exist
INSERT INTO user_profiles (
    id,
    email,
    full_name,
    first_name,
    last_name,
    role,
    is_verified,
    is_active,
    created_at,
    updated_at
) VALUES (
    'admin-' || extract(epoch from now())::text,
    'admin@campus360.com',
    'System Administrator',
    'System',
    'Administrator',
    'admin',
    true,
    true,
    NOW(),
    NOW()
) ON CONFLICT (email) DO UPDATE SET
    full_name = EXCLUDED.full_name,
    role = EXCLUDED.role,
    is_verified = true,
    is_active = true,
    updated_at = NOW();

-- Verify admin user was created/updated
SELECT 
    'Admin user ready' as status,
    id,
    email,
    full_name,
    role,
    is_verified,
    is_active
FROM user_profiles 
WHERE email = 'admin@campus360.com';
