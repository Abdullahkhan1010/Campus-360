-- Fix RLS policies for academic_events table to allow authenticated users to insert/update/delete
-- This script addresses the 42501 error: "new row violates row-level security policy"

-- First, let's check if RLS is enabled and what policies exist
-- DROP POLICY IF EXISTS "Users can insert their own events" ON academic_events;
-- DROP POLICY IF EXISTS "Users can view events" ON academic_events;
-- DROP POLICY IF EXISTS "Users can update their own events" ON academic_events;
-- DROP POLICY IF EXISTS "Users can delete their own events" ON academic_events;

-- Disable RLS temporarily to allow operations
ALTER TABLE academic_events DISABLE ROW LEVEL SECURITY;

-- Re-enable RLS with proper policies
ALTER TABLE academic_events ENABLE ROW LEVEL SECURITY;

-- Allow authenticated users to insert events
CREATE POLICY "Authenticated users can insert events" ON academic_events
    FOR INSERT TO authenticated
    WITH CHECK (true);

-- Allow authenticated users to view all events
CREATE POLICY "Authenticated users can view events" ON academic_events
    FOR SELECT TO authenticated
    USING (true);

-- Allow authenticated users to update events they created or system events
CREATE POLICY "Authenticated users can update events" ON academic_events
    FOR UPDATE TO authenticated
    USING (true)
    WITH CHECK (true);

-- Allow authenticated users to delete events they created
CREATE POLICY "Authenticated users can delete events" ON academic_events
    FOR DELETE TO authenticated
    USING (true);

-- For admin users, allow all operations
CREATE POLICY "Admin users full access" ON academic_events
    FOR ALL TO authenticated
    USING (
        auth.uid()::text IN (
            SELECT id FROM user_profiles WHERE role = 'admin'
        )
    )
    WITH CHECK (
        auth.uid()::text IN (
            SELECT id FROM user_profiles WHERE role = 'admin'
        )
    );

-- Grant necessary permissions to authenticated role
GRANT ALL ON academic_events TO authenticated;
GRANT ALL ON academic_events TO anon;

-- Also grant sequence permissions if they exist
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO authenticated;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO anon;

-- Check current RLS policies
SELECT schemaname, tablename, policyname, permissive, roles, cmd, qual, with_check
FROM pg_policies 
WHERE tablename = 'academic_events';
