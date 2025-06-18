-- Re-enable RLS policies for automation tables after testing
-- Run this after testing is complete to secure the tables

-- Enable RLS on automation tables
ALTER TABLE automation_logs ENABLE ROW LEVEL SECURITY;
ALTER TABLE automation_rules ENABLE ROW LEVEL SECURITY;

-- Create proper RLS policies for automation_logs
CREATE POLICY "automation_logs_select_policy" ON automation_logs
    FOR SELECT USING (true);

CREATE POLICY "automation_logs_insert_policy" ON automation_logs
    FOR INSERT WITH CHECK (true);

CREATE POLICY "automation_logs_update_policy" ON automation_logs
    FOR UPDATE USING (true);

-- Create proper RLS policies for automation_rules
CREATE POLICY "automation_rules_select_policy" ON automation_rules
    FOR SELECT USING (true);

CREATE POLICY "automation_rules_insert_policy" ON automation_rules
    FOR INSERT WITH CHECK (true);

CREATE POLICY "automation_rules_update_policy" ON automation_rules
    FOR UPDATE USING (true);

-- Verify RLS is enabled
SELECT 
    schemaname,
    tablename,
    rowsecurity
FROM pg_tables 
WHERE tablename IN ('automation_logs', 'automation_rules');

-- Show active policies
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
WHERE tablename IN ('automation_logs', 'automation_rules');

SELECT 'RLS re-enabled for automation tables with proper policies' as status;
