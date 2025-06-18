-- Comprehensive RLS fix for all Campus360 tables
-- This addresses permission issues across all tables

-- ========== ACADEMIC_EVENTS TABLE ==========
ALTER TABLE academic_events DISABLE ROW LEVEL SECURITY;
ALTER TABLE academic_events ENABLE ROW LEVEL SECURITY;

-- Drop existing policies
DROP POLICY IF EXISTS "Authenticated users can insert events" ON academic_events;
DROP POLICY IF EXISTS "Authenticated users can view events" ON academic_events;
DROP POLICY IF EXISTS "Authenticated users can update events" ON academic_events;
DROP POLICY IF EXISTS "Authenticated users can delete events" ON academic_events;
DROP POLICY IF EXISTS "Admin users full access" ON academic_events;

-- Create permissive policies for academic_events
CREATE POLICY "Full access for authenticated users" ON academic_events
    FOR ALL TO authenticated
    USING (true)
    WITH CHECK (true);

-- ========== FILE_DOCUMENTS TABLE ==========
-- Check if table exists and fix RLS
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'file_documents') THEN
        ALTER TABLE file_documents DISABLE ROW LEVEL SECURITY;
        ALTER TABLE file_documents ENABLE ROW LEVEL SECURITY;
        
        DROP POLICY IF EXISTS "Full access for authenticated users" ON file_documents;
        
        CREATE POLICY "Full access for authenticated users" ON file_documents
            FOR ALL TO authenticated
            USING (true)
            WITH CHECK (true);
    END IF;
END
$$;

-- ========== USER_PROFILES TABLE ==========
ALTER TABLE user_profiles DISABLE ROW LEVEL SECURITY;
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS "Full access for authenticated users" ON user_profiles;

CREATE POLICY "Full access for authenticated users" ON user_profiles
    FOR ALL TO authenticated
    USING (true)
    WITH CHECK (true);

-- ========== DEPARTMENTS TABLE ==========
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'departments') THEN
        ALTER TABLE departments DISABLE ROW LEVEL SECURITY;
        ALTER TABLE departments ENABLE ROW LEVEL SECURITY;
        
        DROP POLICY IF EXISTS "Full access for authenticated users" ON departments;
        
        CREATE POLICY "Full access for authenticated users" ON departments
            FOR ALL TO authenticated
            USING (true)
            WITH CHECK (true);
    END IF;
END
$$;

-- ========== COURSES TABLE ==========
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'courses') THEN
        ALTER TABLE courses DISABLE ROW LEVEL SECURITY;
        ALTER TABLE courses ENABLE ROW LEVEL SECURITY;
        
        DROP POLICY IF EXISTS "Full access for authenticated users" ON courses;
        
        CREATE POLICY "Full access for authenticated users" ON courses
            FOR ALL TO authenticated
            USING (true)
            WITH CHECK (true);
    END IF;
END
$$;

-- ========== GRANT PERMISSIONS ==========
-- Grant all permissions to authenticated users
GRANT ALL ON ALL TABLES IN SCHEMA public TO authenticated;
GRANT ALL ON ALL SEQUENCES IN SCHEMA public TO authenticated;
GRANT ALL ON ALL FUNCTIONS IN SCHEMA public TO authenticated;

-- Grant permissions to anon (for public access if needed)
GRANT SELECT ON ALL TABLES IN SCHEMA public TO anon;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA public TO anon;

-- ========== VERIFY SETUP ==========
-- Check RLS status
SELECT 
    schemaname, 
    tablename, 
    rowsecurity 
FROM pg_tables 
WHERE schemaname = 'public' 
AND rowsecurity IS NOT NULL;

-- Check policies
SELECT 
    schemaname, 
    tablename, 
    policyname, 
    permissive, 
    roles, 
    cmd, 
    qual, 
    with_check
FROM pg_policies 
WHERE schemaname = 'public'
ORDER BY tablename, policyname;
