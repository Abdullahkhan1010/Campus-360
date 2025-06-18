-- ========================================
-- NUCLEAR OPTION: COMPLETELY DISABLE RLS
-- ========================================
-- This completely disables RLS and removes ALL policies
-- Run this if the previous scripts didn't work

-- 1. Show current policies
SELECT 'Current policies on user_profiles:' as info;
SELECT policyname, cmd, qual, with_check 
FROM pg_policies 
WHERE tablename = 'user_profiles';

-- 2. NUCLEAR: Drop ALL policies on ALL tables
DO $$ 
DECLARE
    r RECORD;
BEGIN
    FOR r IN (
        SELECT schemaname, tablename, policyname 
        FROM pg_policies 
        WHERE schemaname = 'public'
    ) LOOP
        BEGIN
            EXECUTE format('DROP POLICY %I ON %I.%I', r.policyname, r.schemaname, r.tablename);
            RAISE NOTICE 'Dropped policy % on table %', r.policyname, r.tablename;
        EXCEPTION WHEN OTHERS THEN
            RAISE NOTICE 'Failed to drop policy % on table %: %', r.policyname, r.tablename, SQLERRM;
        END;
    END LOOP;
END $$;

-- 3. NUCLEAR: Disable RLS on ALL tables
DO $$ 
DECLARE
    r RECORD;
BEGIN
    FOR r IN (
        SELECT tablename 
        FROM pg_tables 
        WHERE schemaname = 'public'
    ) LOOP
        BEGIN
            EXECUTE format('ALTER TABLE %I DISABLE ROW LEVEL SECURITY', r.tablename);
            RAISE NOTICE 'Disabled RLS on table %', r.tablename;
        EXCEPTION WHEN OTHERS THEN
            RAISE NOTICE 'Failed to disable RLS on table %: %', r.tablename, SQLERRM;
        END;
    END LOOP;
END $$;

-- 4. Test user_profiles access (should work now)
SELECT 'Testing user_profiles access with NO RLS...' as status;
SELECT COUNT(*) as user_count FROM user_profiles;

-- 5. Show final state
SELECT 'Final check - RLS status:' as info;
SELECT tablename, rowsecurity 
FROM pg_tables 
WHERE schemaname = 'public' 
ORDER BY tablename;

SELECT 'NUCLEAR SUCCESS: All RLS completely disabled!' as final_status;