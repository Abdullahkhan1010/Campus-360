-- Temporarily disable RLS policies for automation tables
-- This is for testing purposes only - re-enable RLS in production

-- Disable RLS on automation_logs table
ALTER TABLE automation_logs DISABLE ROW LEVEL SECURITY;

-- Disable RLS on automation_rules table  
ALTER TABLE automation_rules DISABLE ROW LEVEL SECURITY;

-- Drop existing RLS policies if they exist
DROP POLICY IF EXISTS "automation_logs_policy" ON automation_logs;
DROP POLICY IF EXISTS "automation_rules_policy" ON automation_rules;

-- Verify RLS is disabled
SELECT 
    schemaname,
    tablename,
    rowsecurity
FROM pg_tables 
WHERE tablename IN ('automation_logs', 'automation_rules');

-- Grant permissions to authenticated users
GRANT ALL ON automation_logs TO authenticated;
GRANT ALL ON automation_rules TO authenticated;
GRANT ALL ON automation_logs TO anon;
GRANT ALL ON automation_rules TO anon;

-- Grant usage on sequences
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO authenticated;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO anon;

SELECT 'RLS disabled for automation tables - testing can proceed' as status;
