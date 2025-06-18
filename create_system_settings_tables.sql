-- =====================================================
-- Campus360 System Settings and Reporting Database Schema
-- Phase 3: System Settings and Reporting Features
-- =====================================================

-- Drop existing tables if they exist (for development)
DROP TABLE IF EXISTS public.system_settings CASCADE;
DROP TABLE IF EXISTS public.user_preferences CASCADE;
DROP TABLE IF EXISTS public.audit_logs CASCADE;
DROP TABLE IF EXISTS public.system_reports CASCADE;
DROP TABLE IF EXISTS public.report_schedules CASCADE;

-- =====================================================
-- System Settings Table
-- =====================================================
CREATE TABLE public.system_settings (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    category TEXT NOT NULL,
    key TEXT NOT NULL,
    value TEXT NOT NULL,
    value_type TEXT NOT NULL DEFAULT 'string', -- string, number, boolean, json
    description TEXT,
    is_encrypted BOOLEAN DEFAULT false,
    is_public BOOLEAN DEFAULT false, -- whether setting can be accessed by non-admin users    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID REFERENCES auth.users(id),
    updated_by UUID REFERENCES auth.users(id),
    UNIQUE(category, key)
);

-- =====================================================
-- User Preferences Table
-- =====================================================
CREATE TABLE public.user_preferences (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
    category TEXT NOT NULL,
    key TEXT NOT NULL,
    value TEXT NOT NULL,
    value_type TEXT NOT NULL DEFAULT 'string',
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE(user_id, category, key)
);

-- =====================================================
-- Audit Logs Table (for tracking all system changes)
-- =====================================================
CREATE TABLE public.audit_logs (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id UUID REFERENCES auth.users(id),
    action TEXT NOT NULL, -- CREATE, UPDATE, DELETE, LOGIN, LOGOUT, etc.
    entity_type TEXT NOT NULL, -- user, course, assignment, etc.
    entity_id TEXT,
    old_values JSONB,
    new_values JSONB,
    ip_address INET,
    user_agent TEXT,
    session_id TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- =====================================================
-- System Reports Table
-- =====================================================
CREATE TABLE public.system_reports (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT,
    report_type TEXT NOT NULL, -- dashboard, export, scheduled
    category TEXT NOT NULL, -- academic, attendance, performance, system
    query_template TEXT NOT NULL, -- SQL template or report configuration
    parameters JSONB, -- report parameters schema
    access_roles TEXT[] DEFAULT ARRAY['admin'], -- roles that can access this report
    is_active BOOLEAN DEFAULT true,    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID REFERENCES auth.users(id),
    updated_by UUID REFERENCES auth.users(id)
);

-- =====================================================
-- Report Schedules Table
-- =====================================================
CREATE TABLE public.report_schedules (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    report_id UUID NOT NULL REFERENCES public.system_reports(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    schedule_type TEXT NOT NULL, -- daily, weekly, monthly, custom
    cron_expression TEXT, -- for custom schedules
    recipients TEXT[], -- email addresses
    format TEXT DEFAULT 'pdf', -- pdf, excel, csv
    parameters JSONB,
    is_active BOOLEAN DEFAULT true,
    last_run_at TIMESTAMPTZ,
    next_run_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID REFERENCES auth.users(id)
);

-- =====================================================
-- Indexes for Performance
-- =====================================================

-- System Settings Indexes
CREATE INDEX idx_system_settings_category ON public.system_settings(category);
CREATE INDEX idx_system_settings_key ON public.system_settings(key);
CREATE INDEX idx_system_settings_category_key ON public.system_settings(category, key);
CREATE INDEX idx_system_settings_public ON public.system_settings(is_public) WHERE is_public = true;

-- User Preferences Indexes
CREATE INDEX idx_user_preferences_user_id ON public.user_preferences(user_id);
CREATE INDEX idx_user_preferences_category ON public.user_preferences(category);
CREATE INDEX idx_user_preferences_user_category ON public.user_preferences(user_id, category);

-- Audit Logs Indexes
CREATE INDEX idx_audit_logs_user_id ON public.audit_logs(user_id);
CREATE INDEX idx_audit_logs_action ON public.audit_logs(action);
CREATE INDEX idx_audit_logs_entity_type ON public.audit_logs(entity_type);
CREATE INDEX idx_audit_logs_entity_id ON public.audit_logs(entity_id);
CREATE INDEX idx_audit_logs_created_at ON public.audit_logs(created_at);
CREATE INDEX idx_audit_logs_user_action ON public.audit_logs(user_id, action);

-- System Reports Indexes
CREATE INDEX idx_system_reports_type ON public.system_reports(report_type);
CREATE INDEX idx_system_reports_category ON public.system_reports(category);
CREATE INDEX idx_system_reports_active ON public.system_reports(is_active) WHERE is_active = true;

-- Report Schedules Indexes
CREATE INDEX idx_report_schedules_report_id ON public.report_schedules(report_id);
CREATE INDEX idx_report_schedules_active ON public.report_schedules(is_active) WHERE is_active = true;
CREATE INDEX idx_report_schedules_next_run ON public.report_schedules(next_run_at) WHERE is_active = true;

-- =====================================================
-- Row Level Security (RLS) Policies
-- =====================================================

-- Enable RLS
ALTER TABLE public.system_settings ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.user_preferences ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.audit_logs ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.system_reports ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.report_schedules ENABLE ROW LEVEL SECURITY;

-- System Settings Policies
CREATE POLICY "Admin can manage all system settings" ON public.system_settings
    FOR ALL USING (
        (auth.jwt() ->> 'role')::text = 'admin'
    );

CREATE POLICY "Users can read public system settings" ON public.system_settings
    FOR SELECT USING (is_public = true);

-- User Preferences Policies
CREATE POLICY "Users can manage their own preferences" ON public.user_preferences
    FOR ALL USING (user_id = auth.uid());

CREATE POLICY "Admin can read all user preferences" ON public.user_preferences
    FOR SELECT USING (
        (auth.jwt() ->> 'role')::text = 'admin'
    );

-- Audit Logs Policies
CREATE POLICY "Admin can read all audit logs" ON public.audit_logs
    FOR SELECT USING (
        (auth.jwt() ->> 'role')::text = 'admin'
    );

CREATE POLICY "System can create audit logs" ON public.audit_logs
    FOR INSERT WITH CHECK (true);

-- System Reports Policies
CREATE POLICY "Admin can manage all reports" ON public.system_reports
    FOR ALL USING (
        (auth.jwt() ->> 'role')::text = 'admin'
    );

CREATE POLICY "Users can read reports based on access roles" ON public.system_reports
    FOR SELECT USING (
        is_active = true AND (
            (auth.jwt() ->> 'role')::text = ANY(access_roles)
        )
    );

-- Report Schedules Policies
CREATE POLICY "Admin can manage all report schedules" ON public.report_schedules
    FOR ALL USING (
        (auth.jwt() ->> 'role')::text = 'admin'
    );

-- =====================================================
-- Default System Settings
-- =====================================================

INSERT INTO public.system_settings (category, key, value, value_type, description, is_public) VALUES
-- Application Settings
('app', 'name', 'Campus360', 'string', 'Application name', true),
('app', 'version', '1.0.0', 'string', 'Application version', true),
('app', 'description', 'Complete Campus Management System', 'string', 'Application description', true),
('app', 'maintenance_mode', 'false', 'boolean', 'Enable maintenance mode', false),
('app', 'debug_mode', 'false', 'boolean', 'Enable debug logging', false),

-- Academic Settings
('academic', 'current_semester', '1', 'number', 'Current active semester', true),
('academic', 'academic_year', '2024-2025', 'string', 'Current academic year', true),
('academic', 'max_absence_percentage', '25', 'number', 'Maximum allowed absence percentage', true),
('academic', 'passing_grade', '40', 'number', 'Minimum passing grade percentage', true),
('academic', 'grade_scale', '{"A+":90,"A":85,"B+":80,"B":75,"C+":70,"C":65,"D":50,"F":0}', 'json', 'Grading scale configuration', true),

-- Notification Settings
('notifications', 'email_enabled', 'true', 'boolean', 'Enable email notifications', false),
('notifications', 'sms_enabled', 'false', 'boolean', 'Enable SMS notifications', false),
('notifications', 'push_enabled', 'true', 'boolean', 'Enable push notifications', false),
('notifications', 'assignment_reminder_days', '3', 'number', 'Days before assignment due date to send reminder', true),
('notifications', 'low_attendance_threshold', '75', 'number', 'Attendance percentage threshold for alerts', true),

-- Automation Settings
('automation', 'enabled', 'true', 'boolean', 'Enable automation engine', false),
('automation', 'log_retention_days', '90', 'number', 'Number of days to retain automation logs', false),
('automation', 'max_retry_attempts', '3', 'number', 'Maximum retry attempts for failed automations', false),
('automation', 'retry_delay_minutes', '5', 'number', 'Delay in minutes between retry attempts', false),

-- Security Settings
('security', 'session_timeout_minutes', '60', 'number', 'Session timeout in minutes', false),
('security', 'max_login_attempts', '5', 'number', 'Maximum failed login attempts before lockout', false),
('security', 'lockout_duration_minutes', '30', 'number', 'Account lockout duration in minutes', false),
('security', 'password_min_length', '8', 'number', 'Minimum password length', true),
('security', 'require_2fa', 'false', 'boolean', 'Require two-factor authentication', true),

-- File Upload Settings
('uploads', 'max_file_size_mb', '10', 'number', 'Maximum file upload size in MB', true),
('uploads', 'allowed_extensions', '["pdf","doc","docx","jpg","jpeg","png","gif","zip"]', 'json', 'Allowed file extensions', true),
('uploads', 'storage_path', '/uploads', 'string', 'File storage path', false),

-- Performance Settings
('performance', 'cache_enabled', 'true', 'boolean', 'Enable application caching', false),
('performance', 'cache_duration_minutes', '30', 'number', 'Cache duration in minutes', false),
('performance', 'max_page_size', '100', 'number', 'Maximum items per page in lists', true),
('performance', 'api_rate_limit', '1000', 'number', 'API rate limit per hour', false);

-- =====================================================
-- Default System Reports
-- =====================================================

INSERT INTO public.system_reports (name, description, report_type, category, query_template, access_roles) VALUES
-- Academic Reports
('Student Performance Summary', 'Overall student performance across all courses', 'dashboard', 'academic', 
 'SELECT * FROM student_performance_view', ARRAY['admin', 'teacher']),

('Course Enrollment Report', 'Student enrollment statistics by course', 'export', 'academic', 
 'SELECT * FROM course_enrollment_view', ARRAY['admin']),

('Assignment Submission Report', 'Assignment submission rates and statistics', 'export', 'academic', 
 'SELECT * FROM assignment_submission_view', ARRAY['admin', 'teacher']),

-- Attendance Reports
('Daily Attendance Report', 'Daily attendance summary for all classes', 'scheduled', 'attendance', 
 'SELECT * FROM daily_attendance_view', ARRAY['admin', 'teacher']),

('Low Attendance Alert Report', 'Students with attendance below threshold', 'dashboard', 'attendance', 
 'SELECT * FROM low_attendance_view', ARRAY['admin', 'teacher']),

-- System Reports
('User Activity Report', 'System usage and user activity statistics', 'export', 'system', 
 'SELECT * FROM user_activity_view', ARRAY['admin']),

('Automation Logs Report', 'Automation system performance and logs', 'dashboard', 'system', 
 'SELECT * FROM automation_logs_view', ARRAY['admin']),

('System Health Report', 'Overall system health and performance metrics', 'scheduled', 'system', 
 'SELECT * FROM system_health_view', ARRAY['admin']);

-- =====================================================
-- Verification Queries
-- =====================================================

-- Verify table creation
SELECT 
    schemaname,
    tablename,
    tableowner,
    hasindexes,
    hasrules
FROM pg_tables 
WHERE schemaname = 'public' 
AND tablename IN ('system_settings', 'user_preferences', 'audit_logs', 'system_reports', 'report_schedules')
ORDER BY tablename;

-- Verify default settings
SELECT category, key, value, is_public 
FROM public.system_settings 
ORDER BY category, key;

-- Verify default reports
SELECT name, category, report_type, access_roles 
FROM public.system_reports 
ORDER BY category, name;

-- Check RLS status
SELECT 
    schemaname,
    tablename,
    rowsecurity
FROM pg_tables 
WHERE schemaname = 'public' 
AND tablename IN ('system_settings', 'user_preferences', 'audit_logs', 'system_reports', 'report_schedules')
ORDER BY tablename;

COMMIT;
