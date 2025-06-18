-- Setup Admin User for Campus 360
-- Run this in your Supabase SQL Editor

-- First, create the user_profiles table if it doesn't exist
CREATE TABLE IF NOT EXISTS user_profiles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    full_name VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    role VARCHAR(50) NOT NULL DEFAULT 'student',
    department_id UUID,
    phone VARCHAR(20),
    date_of_birth DATE,
    gender VARCHAR(10),
    address TEXT,
    student_id VARCHAR(50),
    program VARCHAR(100),
    academic_year VARCHAR(20),
    admission_date DATE,
    emergency_contact_name VARCHAR(255),
    emergency_contact_phone VARCHAR(20),
    emergency_contact_relationship VARCHAR(50),
    is_verified BOOLEAN DEFAULT false,
    is_active BOOLEAN DEFAULT true,
    last_login_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Enable Row Level Security
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;

-- Create RLS policies
CREATE POLICY "Users can view own profile" ON user_profiles
    FOR SELECT USING (auth.uid()::text = id::text);

CREATE POLICY "Users can update own profile" ON user_profiles
    FOR UPDATE USING (auth.uid()::text = id::text);

CREATE POLICY "Admins can view all profiles" ON user_profiles
    FOR SELECT USING (
        EXISTS (
            SELECT 1 FROM user_profiles 
            WHERE id::text = auth.uid()::text AND role = 'admin'
        )
    );

CREATE POLICY "Admins can manage all profiles" ON user_profiles
    FOR ALL USING (
        EXISTS (
            SELECT 1 FROM user_profiles 
            WHERE id::text = auth.uid()::text AND role = 'admin'
        )
    );

-- Insert admin user profile (you'll need to update the ID after creating the auth user)
-- Note: Replace 'USER_UUID_FROM_AUTH' with the actual UUID from the auth.users table
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
    'USER_UUID_FROM_AUTH'::uuid,  -- Replace with actual UUID from auth.users
    'admin@campus360.com',
    'Campus Administrator',
    'Campus',
    'Administrator', 
    'admin',
    true,
    true,
    NOW(),
    NOW()
) ON CONFLICT (email) DO UPDATE SET
    full_name = EXCLUDED.full_name,
    role = EXCLUDED.role,
    is_verified = EXCLUDED.is_verified,
    updated_at = NOW();

-- Create departments table if needed
CREATE TABLE IF NOT EXISTS departments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    code VARCHAR(10) UNIQUE NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Insert sample departments
INSERT INTO departments (name, code, description) VALUES
    ('Computer Science', 'CS', 'Computer Science Department'),
    ('Information Technology', 'IT', 'Information Technology Department'),
    ('Software Engineering', 'SE', 'Software Engineering Department'),
    ('Artificial Intelligence', 'AI', 'Artificial Intelligence Department')
ON CONFLICT (code) DO NOTHING;
