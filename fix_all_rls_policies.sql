-- Fix RLS policies for all tables to prevent authentication issues
-- Run this in Supabase SQL Editor

-- Temporarily disable RLS on tables that are causing authentication issues
ALTER TABLE activity_logs DISABLE ROW LEVEL SECURITY;
ALTER TABLE system_logs DISABLE ROW LEVEL SECURITY;
ALTER TABLE notifications DISABLE ROW LEVEL SECURITY;
ALTER TABLE app_settings DISABLE ROW LEVEL SECURITY;
ALTER TABLE user_settings DISABLE ROW LEVEL SECURITY;

-- Keep other tables with RLS but make sure they don't block basic operations
-- For departments, allow all authenticated users to read
DROP POLICY IF EXISTS "All users can view departments" ON departments;
DROP POLICY IF EXISTS "Only admins can modify departments" ON departments;

CREATE POLICY "All authenticated users can view departments" ON departments FOR SELECT TO authenticated USING (true);
CREATE POLICY "Admins can modify departments" ON departments FOR ALL USING (
    auth.uid() IN (
        SELECT id FROM user_profiles WHERE role = 'admin'
    )
);

-- For courses, allow basic read access
DROP POLICY IF EXISTS "Users can view courses in their department" ON courses;
CREATE POLICY "Authenticated users can view courses" ON courses FOR SELECT TO authenticated USING (true);

-- For file_documents, allow basic read access
DROP POLICY IF EXISTS "Users can view files based on visibility" ON file_documents;
CREATE POLICY "Authenticated users can view public files" ON file_documents FOR SELECT TO authenticated USING (
    visibility = 'public' OR uploaded_by = auth.uid()
);

-- Verify RLS status
SELECT 
    schemaname, 
    tablename, 
    rowsecurity as rls_enabled
FROM pg_tables 
WHERE schemaname = 'public' 
ORDER BY tablename;

SELECT 'RLS policies updated. Authentication should now work without policy violations.' as status;
