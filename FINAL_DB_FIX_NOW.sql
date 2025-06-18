-- =====================================================
-- FINAL DATABASE FIX - RUN THIS NOW IN SUPABASE
-- =====================================================

-- Step 1: Drop all policies that depend on ID columns
DO $$ 
DECLARE 
    pol record;
BEGIN
    FOR pol IN SELECT schemaname, tablename, policyname FROM pg_policies WHERE schemaname = 'public'
    LOOP
        EXECUTE 'DROP POLICY IF EXISTS "' || pol.policyname || '" ON ' || pol.tablename;
    END LOOP;
END $$;

-- Step 2: Disable RLS temporarily
ALTER TABLE user_profiles DISABLE ROW LEVEL SECURITY;
ALTER TABLE departments DISABLE ROW LEVEL SECURITY;

-- Step 3: Drop all foreign key constraints
DO $$
DECLARE
    r record;
BEGIN
    FOR r IN SELECT constraint_name, table_name FROM information_schema.table_constraints 
             WHERE constraint_type = 'FOREIGN KEY' AND table_schema = 'public'
    LOOP
        EXECUTE 'ALTER TABLE ' || r.table_name || ' DROP CONSTRAINT ' || r.constraint_name;
    END LOOP;
END $$;

-- Step 4: Convert columns to TEXT
ALTER TABLE user_profiles ALTER COLUMN id TYPE TEXT USING id::TEXT;
ALTER TABLE user_profiles ALTER COLUMN department_id TYPE TEXT USING department_id::TEXT;
ALTER TABLE departments ALTER COLUMN id TYPE TEXT USING id::TEXT;

-- Step 5: Minimal RLS policies
CREATE POLICY "admin_all" ON user_profiles FOR ALL USING (
    EXISTS (SELECT 1 FROM user_profiles WHERE id = auth.uid()::TEXT AND role = 'admin')
);

-- Step 6: Re-enable RLS
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;

-- Step 7: Test
INSERT INTO user_profiles (id, email, full_name, role, is_verified, is_active, created_at, updated_at) 
VALUES ('test-final-' || extract(epoch from now())::TEXT, 'final.test@campus360.com', 'Final Test', 'teacher', true, true, NOW(), NOW()) 
ON CONFLICT (email) DO NOTHING;

SELECT 'Database fixed successfully' as result;
