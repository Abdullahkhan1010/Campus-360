-- ===================================================================
-- AUTOMATION LOG MANAGEMENT - SIMPLE TABLE CREATION (NO FOREIGN KEYS)
-- ===================================================================
-- This script creates the automation_logs and automation_rules tables
-- without foreign key constraints to avoid type mismatch issues
-- Run this in Supabase SQL Editor first

-- Drop tables if they exist (for testing)
DROP TABLE IF EXISTS automation_logs CASCADE;
DROP TABLE IF EXISTS automation_rules CASCADE;

-- Create automation_logs table (no foreign keys)
CREATE TABLE automation_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    log_id VARCHAR(100) NOT NULL UNIQUE,
    trigger_type VARCHAR(50) NOT NULL CHECK (trigger_type IN (
        'AttendanceBelowThreshold', 'ResultUploaded', 'AssignmentUploaded',
        'AssignmentDeadlineApproaching', 'ClassCancelled', 'NoticePublished',
        'CustomEvent', 'SystemMaintenance', 'UserLogin', 'FileUploaded'
    )),
    rule_id VARCHAR(100) NOT NULL,
    rule_name VARCHAR(255) NOT NULL,
    title VARCHAR(255) NOT NULL,
    message TEXT NOT NULL,
    
    -- Course related fields
    course_id TEXT,
    course_name VARCHAR(255),
    course_code VARCHAR(20),
    
    -- User related fields
    student_id TEXT,
    student_name VARCHAR(255),
    teacher_id TEXT,
    teacher_name VARCHAR(255),
    
    -- Execution details
    status VARCHAR(20) DEFAULT 'pending' CHECK (status IN ('pending', 'sent', 'delivered', 'failed', 'retrying')),
    triggered_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    triggered_by VARCHAR(100) DEFAULT 'system',
    target_user_id TEXT,
    target_type VARCHAR(50) DEFAULT 'user',
    action_taken TEXT,
    
    -- Result tracking
    is_successful BOOLEAN DEFAULT TRUE,
    result_data TEXT,
    sent_at TIMESTAMP WITH TIME ZONE,
    delivered_at TIMESTAMP WITH TIME ZONE,
    trigger_reason TEXT,
    error_message TEXT,
    retry_count INTEGER DEFAULT 0,
    
    -- Metadata
    related_entity_id TEXT,
    related_entity_type VARCHAR(50),
    metadata JSONB DEFAULT '{}',
    recipient_count INTEGER DEFAULT 1,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create automation_rules table (no foreign keys)
CREATE TABLE automation_rules (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    rule_id VARCHAR(100) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    trigger_type VARCHAR(50) NOT NULL,
    trigger_condition TEXT,
    message_template TEXT NOT NULL,
    notification_type VARCHAR(50) NOT NULL,
    priority VARCHAR(20) DEFAULT 'normal' CHECK (priority IN ('low', 'normal', 'high', 'critical')),
    is_active BOOLEAN DEFAULT TRUE,
    target_role VARCHAR(50),
    delay_before_trigger INTERVAL,
    trigger_count INTEGER DEFAULT 0,
    last_triggered TIMESTAMP WITH TIME ZONE,
    created_by TEXT DEFAULT 'system',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create indices for performance
CREATE INDEX idx_automation_logs_trigger_type ON automation_logs(trigger_type);
CREATE INDEX idx_automation_logs_rule_id ON automation_logs(rule_id);
CREATE INDEX idx_automation_logs_course_id ON automation_logs(course_id);
CREATE INDEX idx_automation_logs_student_id ON automation_logs(student_id);
CREATE INDEX idx_automation_logs_teacher_id ON automation_logs(teacher_id);
CREATE INDEX idx_automation_logs_target_user_id ON automation_logs(target_user_id);
CREATE INDEX idx_automation_logs_status ON automation_logs(status);
CREATE INDEX idx_automation_logs_created_at ON automation_logs(created_at DESC);
CREATE INDEX idx_automation_logs_triggered_at ON automation_logs(triggered_at DESC);

-- Create composite indices for common queries
CREATE INDEX idx_automation_logs_user_status ON automation_logs(target_user_id, status);
CREATE INDEX idx_automation_logs_course_status ON automation_logs(course_id, status);
CREATE INDEX idx_automation_logs_type_status ON automation_logs(trigger_type, status);

-- Create indices for automation rules
CREATE INDEX idx_automation_rules_trigger_type ON automation_rules(trigger_type);
CREATE INDEX idx_automation_rules_is_active ON automation_rules(is_active);
CREATE INDEX idx_automation_rules_priority ON automation_rules(priority);

-- Enable Row Level Security
ALTER TABLE automation_logs ENABLE ROW LEVEL SECURITY;
ALTER TABLE automation_rules ENABLE ROW LEVEL SECURITY;

-- Create RLS policies for automation logs
CREATE POLICY "Users can view their own automation logs" ON automation_logs
    FOR SELECT USING (
        auth.uid()::text IN (target_user_id, student_id, teacher_id)
    );

CREATE POLICY "System can insert automation logs" ON automation_logs
    FOR INSERT WITH CHECK (true);

CREATE POLICY "System can update automation logs" ON automation_logs
    FOR UPDATE USING (true);

-- Create RLS policies for automation rules
CREATE POLICY "Everyone can view active automation rules" ON automation_rules
    FOR SELECT USING (is_active = TRUE);

CREATE POLICY "Admins can manage automation rules" ON automation_rules
    FOR ALL USING (
        EXISTS (
            SELECT 1 FROM user_profiles 
            WHERE id = auth.uid()::text 
            AND role = 'admin'
        )
    );

-- Insert default automation rules
INSERT INTO automation_rules (rule_id, name, description, trigger_type, trigger_condition, message_template, notification_type, priority, is_active, created_by) VALUES
('rule1', 'Low Attendance Alert', 'Notify students when attendance drops below 75%', 'AttendanceBelowThreshold', 'attendance_percentage < 75', '⚠️ Your attendance in {course_name} is now {attendance_percentage}%. Improve to avoid shortage.', 'Warning', 'high', TRUE, 'system'),
('rule2', 'Result Upload Notification', 'Notify students when new results are uploaded', 'ResultUploaded', 'new_result_uploaded', '📊 Your {exam_type} result for {course_name}: {marks_obtained}/{total_marks} ({grade})', 'Result', 'normal', TRUE, 'system'),
('rule3', 'Assignment Upload Alert', 'Notify students when new assignments are uploaded', 'AssignmentUploaded', 'new_assignment_created', '📝 New Assignment: {assignment_title} for {course_name}. Due: {due_date}', 'Assignment', 'normal', TRUE, 'system'),
('rule4', 'Assignment Deadline Reminder', 'Remind students 24 hours before assignment deadline', 'AssignmentDeadlineApproaching', 'hours_until_deadline <= 24', '⌛️ Reminder: {assignment_title} for {course_name} is due in {hours_remaining} hours.', 'Deadline', 'high', TRUE, 'system'),
('rule5', 'Class Cancellation Notice', 'Notify students when class is cancelled', 'ClassCancelled', 'class_cancelled', '❌ {course_name} class scheduled for {class_date} has been cancelled. {reason}', 'Alert', 'critical', TRUE, 'system'),
('rule6', 'Notice Publication Alert', 'Notify users when new notices are published', 'NoticePublished', 'new_notice_published', '📢 New Notice: {notice_title}. {notice_content}', 'Notice', 'normal', TRUE, 'system');

-- Create function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_automation_logs_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create triggers for updated_at
CREATE TRIGGER update_automation_logs_updated_at
    BEFORE UPDATE ON automation_logs
    FOR EACH ROW
    EXECUTE FUNCTION update_automation_logs_updated_at();

CREATE TRIGGER update_automation_rules_updated_at
    BEFORE UPDATE ON automation_rules
    FOR EACH ROW
    EXECUTE FUNCTION update_automation_logs_updated_at();

-- Verify tables were created
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('automation_logs', 'automation_rules')
ORDER BY table_name;

-- Check RLS status for the tables
SELECT 
    schemaname,
    tablename,
    rowsecurity
FROM pg_tables 
WHERE schemaname = 'public' 
AND tablename IN ('automation_logs', 'automation_rules')
ORDER BY tablename;

-- Display success message
SELECT 'Automation Log Management tables created successfully!' as status;
