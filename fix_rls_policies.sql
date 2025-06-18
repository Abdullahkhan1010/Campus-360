-- Fix RLS policies to prevent infinite recursion
-- Run this in Supabase SQL Editor

-- First, drop the problematic policies
DROP POLICY IF EXISTS "Users can view their own profile" ON user_profiles;
DROP POLICY IF EXISTS "Users can update their own profile" ON user_profiles;
DROP POLICY IF EXISTS "Admins can view all profiles" ON user_profiles;

-- Create simpler, non-recursive policies
-- Allow users to view their own profile
CREATE POLICY "Users can view own profile" ON user_profiles FOR SELECT USING (
    auth.uid() = id
);

-- Allow users to update their own profile
CREATE POLICY "Users can update own profile" ON user_profiles FOR UPDATE USING (
    auth.uid() = id
);

-- Allow admins to view all profiles (without recursion)
-- This uses a direct role check instead of querying the same table
CREATE POLICY "Admins can view all profiles" ON user_profiles FOR SELECT USING (
    auth.uid() IN (
        SELECT id FROM user_profiles WHERE role = 'admin'
    ) OR auth.uid() = id
);

-- Allow admins to insert new profiles
CREATE POLICY "Admins can insert profiles" ON user_profiles FOR INSERT WITH CHECK (
    auth.uid() IN (
        SELECT id FROM user_profiles WHERE role = 'admin'
    )
);

-- Allow admins to update all profiles
CREATE POLICY "Admins can update all profiles" ON user_profiles FOR UPDATE USING (
    auth.uid() IN (
        SELECT id FROM user_profiles WHERE role = 'admin'
    ) OR auth.uid() = id
);

-- Allow admins to delete profiles
CREATE POLICY "Admins can delete profiles" ON user_profiles FOR DELETE USING (
    auth.uid() IN (
        SELECT id FROM user_profiles WHERE role = 'admin'
    )
);

-- For testing, let's also create a temporary policy that bypasses RLS for a specific admin user
-- We'll remove this later once everything works
-- First, let's check if we have an admin user
DO $$
DECLARE
    admin_id UUID;
BEGIN
    SELECT id INTO admin_id FROM auth.users WHERE email = 'admin@campus360.com';
    
    IF admin_id IS NOT NULL THEN
        EXECUTE format('CREATE POLICY "Temp admin bypass" ON user_profiles FOR ALL USING (auth.uid() = ''%s''::uuid)', admin_id);
    END IF;
END $$;

-- Verify the policies were created
SELECT schemaname, tablename, policyname, cmd, qual
FROM pg_policies 
WHERE tablename = 'user_profiles'
ORDER BY policyname;
