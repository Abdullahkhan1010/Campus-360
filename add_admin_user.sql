-- ===================================================================
-- ADD ADMIN USER TO CAMPUS 360
-- ===================================================================
-- Run this AFTER running database_schema_complete.sql
-- This script creates the admin user for testing

-- Step 1: First, create the auth user via Supabase Dashboard
-- Go to Authentication > Users > Add User
-- Email: admin@campus360.com
-- Password: admin123
-- Auto Confirm User: YES
-- Copy the generated UUID

-- Step 2: Insert admin user profile
-- Replace 'REPLACE_WITH_AUTH_USER_UUID' with the actual UUID from step 1
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
) VALUES (
    '42b2297a-b944-403e-add4-49ced0d81bb4'::uuid,  -- ⚠️ REPLACE THIS WITH ACTUAL UUID
    'admin@campus360.com',
    'Campus Administrator',
    'Campus',
    'Administrator',
    'admin',
    (SELECT id FROM departments WHERE code = 'ADMIN' LIMIT 1),  -- Admin department
    true,
    true,
    NOW(),
    NOW()
) ON CONFLICT (email) DO UPDATE SET
    full_name = EXCLUDED.full_name,
    role = EXCLUDED.role,
    is_verified = EXCLUDED.is_verified,
    updated_at = NOW();

-- Optional: Add sample teacher and student users
-- (You can create these via the dashboard later if needed)

-- Success message
SELECT 'Admin user profile created successfully!' as status,
       'Email: admin@campus360.com' as credentials,
       'Password: admin123' as password;
