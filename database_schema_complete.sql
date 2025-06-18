-- ===================================================================
-- CAMPUS 360 - COMPLETE SUPABASE DATABASE SCHEMA
-- ===================================================================
-- This schema supports the full Campus 360 application with all features
-- Created for Supabase PostgreSQL database
-- ===================================================================

-- Enable necessary extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ===================================================================
-- 1. CORE USER MANAGEMENT TABLES
-- ===================================================================

-- Departments (must be created first due to foreign key dependency)
CREATE TABLE departments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    code VARCHAR(10) UNIQUE NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- User Profiles (extends Supabase auth.users)
CREATE TABLE user_profiles (
    id UUID PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
    email VARCHAR(255) UNIQUE NOT NULL,
    full_name VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    role VARCHAR(20) NOT NULL CHECK (role IN ('admin', 'teacher', 'student')),
    department_id UUID REFERENCES departments(id),
    phone VARCHAR(20),
    date_of_birth DATE,
    gender VARCHAR(10) CHECK (gender IN ('male', 'female', 'other')),
    address TEXT,
    student_id VARCHAR(50) UNIQUE,
    program VARCHAR(100),
    academic_year VARCHAR(20),
    admission_date DATE,
    emergency_contact_name VARCHAR(255),
    emergency_contact_phone VARCHAR(20),
    emergency_contact_relationship VARCHAR(50),
    is_verified BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    last_login_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ===================================================================
-- 2. ACADEMIC MANAGEMENT TABLES
-- ===================================================================

-- Courses
CREATE TABLE courses (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    code VARCHAR(20) UNIQUE NOT NULL,
    description TEXT,
    department_id UUID NOT NULL REFERENCES departments(id),
    teacher_id UUID REFERENCES user_profiles(id),
    semester INTEGER NOT NULL CHECK (semester BETWEEN 1 AND 8),
    credit_hours INTEGER NOT NULL CHECK (credit_hours BETWEEN 1 AND 10),
    academic_year VARCHAR(20),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Course Enrollments (Many-to-Many: Students to Courses)
CREATE TABLE course_enrollments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    student_id UUID NOT NULL REFERENCES user_profiles(id),
    course_id UUID NOT NULL REFERENCES courses(id),
    enrolled_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    UNIQUE(student_id, course_id)
);

-- Assignments
CREATE TABLE assignments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    course_id UUID NOT NULL REFERENCES courses(id),
    title VARCHAR(255) NOT NULL,
    description TEXT,
    due_date TIMESTAMP WITH TIME ZONE NOT NULL,
    max_score INTEGER DEFAULT 100,
    assignment_type VARCHAR(50) DEFAULT 'homework' CHECK (assignment_type IN ('homework', 'quiz', 'project', 'exam')),
    is_published BOOLEAN DEFAULT FALSE,
    created_by UUID NOT NULL REFERENCES user_profiles(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Assignment Submissions
CREATE TABLE assignment_submissions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    assignment_id UUID NOT NULL REFERENCES assignments(id),
    student_id UUID NOT NULL REFERENCES user_profiles(id),
    submission_text TEXT,
    file_attachments JSONB DEFAULT '[]',
    submitted_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    score INTEGER,
    feedback TEXT,
    graded_by UUID REFERENCES user_profiles(id),
    graded_at TIMESTAMP WITH TIME ZONE,
    is_late BOOLEAN DEFAULT FALSE,
    UNIQUE(assignment_id, student_id)
);

-- ===================================================================
-- 3. ATTENDANCE MANAGEMENT TABLES
-- ===================================================================

-- Attendance Sessions
CREATE TABLE attendance_sessions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    course_id UUID NOT NULL REFERENCES courses(id),
    class_date DATE NOT NULL,
    class_type VARCHAR(20) DEFAULT 'lecture' CHECK (class_type IN ('lecture', 'lab', 'tutorial', 'seminar')),
    topic VARCHAR(255),
    is_completed BOOLEAN DEFAULT FALSE,
    total_students INTEGER DEFAULT 0,
    present_students INTEGER DEFAULT 0,
    absent_students INTEGER DEFAULT 0,
    created_by UUID NOT NULL REFERENCES user_profiles(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Attendance Records
CREATE TABLE attendance_records (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    session_id UUID NOT NULL REFERENCES attendance_sessions(id),
    student_id UUID NOT NULL REFERENCES user_profiles(id),
    is_present BOOLEAN NOT NULL DEFAULT FALSE,
    remarks TEXT,
    marked_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    marked_by UUID NOT NULL REFERENCES user_profiles(id),
    UNIQUE(session_id, student_id)
);

-- ===================================================================
-- 4. CALENDAR & EVENTS SYSTEM
-- ===================================================================

-- Academic Events
CREATE TABLE academic_events (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title VARCHAR(255) NOT NULL,
    description TEXT,
    event_type VARCHAR(50) NOT NULL CHECK (event_type IN ('assignment', 'exam', 'notice', 'result', 'holiday', 'meeting', 'custom')),
    category VARCHAR(50) DEFAULT 'academic',
    priority VARCHAR(20) DEFAULT 'normal' CHECK (priority IN ('low', 'normal', 'high', 'urgent')),
    start_date TIMESTAMP WITH TIME ZONE NOT NULL,
    end_date TIMESTAMP WITH TIME ZONE,
    start_time TIME,
    end_time TIME,
    is_all_day BOOLEAN DEFAULT FALSE,
    venue VARCHAR(255),
    room VARCHAR(100),
    building VARCHAR(100),
    online_link TEXT,
    course_id UUID REFERENCES courses(id),
    department_id UUID REFERENCES departments(id),
    target_role VARCHAR(20) DEFAULT 'all' CHECK (target_role IN ('admin', 'teacher', 'student', 'all')),
    target_user_ids JSONB DEFAULT '[]',
    target_course_ids JSONB DEFAULT '[]',
    target_department_ids JSONB DEFAULT '[]',
    created_by UUID NOT NULL REFERENCES user_profiles(id),
    created_by_role VARCHAR(20),
    status VARCHAR(20) DEFAULT 'active' CHECK (status IN ('active', 'cancelled', 'completed', 'postponed')),
    is_published BOOLEAN DEFAULT TRUE,
    is_recurring BOOLEAN DEFAULT FALSE,
    recurring_pattern JSONB,
    is_system_generated BOOLEAN DEFAULT FALSE,
    source_type VARCHAR(50),
    source_id UUID,
    automation_rule_id UUID,
    has_reminder BOOLEAN DEFAULT FALSE,
    color VARCHAR(7) DEFAULT '#007bff',
    icon_class VARCHAR(50),
    badge_class VARCHAR(50),
    metadata JSONB DEFAULT '{}',
    tags JSONB DEFAULT '[]',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Event Reminders
CREATE TABLE event_reminders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    event_id UUID NOT NULL REFERENCES academic_events(id) ON DELETE CASCADE,
    reminder_type VARCHAR(20) NOT NULL CHECK (reminder_type IN ('email', 'notification', 'sms')),
    reminder_time INTEGER NOT NULL, -- minutes before event
    is_sent BOOLEAN DEFAULT FALSE,
    sent_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ===================================================================
-- 5. NOTIFICATION SYSTEM
-- ===================================================================

-- Notifications
CREATE TABLE notifications (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES user_profiles(id),
    title VARCHAR(255) NOT NULL,
    message TEXT NOT NULL,
    type VARCHAR(20) DEFAULT 'info' CHECK (type IN ('info', 'success', 'warning', 'error', 'result', 'assignment', 'attendance', 'deadline', 'alert', 'notice')),
    category VARCHAR(50) DEFAULT 'general' CHECK (category IN ('general', 'academic', 'administrative', 'system', 'automation')),
    priority VARCHAR(20) DEFAULT 'normal' CHECK (priority IN ('low', 'normal', 'high', 'urgent')),
    related_id UUID,
    action_url TEXT,
    icon_class VARCHAR(50),
    badge_class VARCHAR(50),
    course_id UUID REFERENCES courses(id),
    is_read BOOLEAN DEFAULT FALSE,
    read_at TIMESTAMP WITH TIME ZONE,
    is_system_generated BOOLEAN DEFAULT TRUE,
    generated_by UUID REFERENCES user_profiles(id),
    scheduled_for TIMESTAMP WITH TIME ZONE,
    is_sent BOOLEAN DEFAULT FALSE,
    sent_at TIMESTAMP WITH TIME ZONE,
    expires_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ===================================================================
-- 6. FILE MANAGEMENT SYSTEM
-- ===================================================================

-- File Documents
CREATE TABLE file_documents (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    file_name VARCHAR(255) NOT NULL,
    original_file_name VARCHAR(255) NOT NULL,
    file_url TEXT NOT NULL,
    file_type VARCHAR(50) NOT NULL CHECK (file_type IN ('pdf', 'doc', 'docx', 'ppt', 'pptx', 'xls', 'xlsx', 'image', 'video', 'audio', 'archive', 'other')),
    uploaded_by UUID NOT NULL REFERENCES user_profiles(id),
    uploader_name VARCHAR(255),
    subject_id UUID,
    subject_name VARCHAR(255),
    course_id UUID REFERENCES courses(id),
    course_name VARCHAR(255),
    department_id UUID REFERENCES departments(id),
    department_name VARCHAR(255),
    visibility VARCHAR(20) DEFAULT 'course' CHECK (visibility IN ('public', 'department', 'course', 'private')),
    description TEXT,
    file_size BIGINT DEFAULT 0,
    content_type VARCHAR(100),
    download_count INTEGER DEFAULT 0,
    last_downloaded TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN DEFAULT TRUE,
    is_featured BOOLEAN DEFAULT FALSE,
    due_date TIMESTAMP WITH TIME ZONE,
    max_score INTEGER,
    tags JSONB DEFAULT '[]',
    metadata JSONB DEFAULT '{}',
    upload_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_modified TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- File Access Logs
CREATE TABLE file_access_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    file_id UUID NOT NULL REFERENCES file_documents(id),
    user_id UUID NOT NULL REFERENCES user_profiles(id),
    access_type VARCHAR(20) NOT NULL CHECK (access_type IN ('view', 'download', 'edit', 'delete')),
    ip_address INET,
    user_agent TEXT,
    accessed_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ===================================================================
-- 7. AUDIT & ACTIVITY LOGGING
-- ===================================================================

-- Activity Logs
CREATE TABLE activity_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES user_profiles(id),
    title VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    activity_type VARCHAR(50) NOT NULL,
    table_name VARCHAR(100),
    record_id UUID,
    old_values JSONB,
    new_values JSONB,
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- System Logs
CREATE TABLE system_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    level VARCHAR(20) NOT NULL CHECK (level IN ('debug', 'info', 'warning', 'error', 'critical')),
    message TEXT NOT NULL,
    component VARCHAR(100),
    user_id UUID REFERENCES user_profiles(id),
    context JSONB DEFAULT '{}',
    stack_trace TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- ===================================================================
-- 8. CONFIGURATION & SETTINGS
-- ===================================================================

-- Application Settings
CREATE TABLE app_settings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    key VARCHAR(100) UNIQUE NOT NULL,
    value TEXT,
    description TEXT,
    data_type VARCHAR(20) DEFAULT 'string' CHECK (data_type IN ('string', 'number', 'boolean', 'json')),
    is_public BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- User Settings
CREATE TABLE user_settings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES user_profiles(id),
    key VARCHAR(100) NOT NULL,
    value TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, key)
);

-- ===================================================================
-- 9. INDEXES FOR PERFORMANCE
-- ===================================================================

-- User Profiles Indexes
CREATE INDEX idx_user_profiles_email ON user_profiles(email);
CREATE INDEX idx_user_profiles_role ON user_profiles(role);
CREATE INDEX idx_user_profiles_department ON user_profiles(department_id);
CREATE INDEX idx_user_profiles_student_id ON user_profiles(student_id);

-- Courses Indexes
CREATE INDEX idx_courses_department ON courses(department_id);
CREATE INDEX idx_courses_teacher ON courses(teacher_id);
CREATE INDEX idx_courses_active ON courses(is_active);
CREATE INDEX idx_courses_code ON courses(code);

-- Assignments Indexes
CREATE INDEX idx_assignments_course ON assignments(course_id);
CREATE INDEX idx_assignments_due_date ON assignments(due_date);
CREATE INDEX idx_assignments_created_by ON assignments(created_by);

-- Attendance Indexes
CREATE INDEX idx_attendance_sessions_course ON attendance_sessions(course_id);
CREATE INDEX idx_attendance_sessions_date ON attendance_sessions(class_date);
CREATE INDEX idx_attendance_records_session ON attendance_records(session_id);
CREATE INDEX idx_attendance_records_student ON attendance_records(student_id);

-- Events Indexes
CREATE INDEX idx_academic_events_start_date ON academic_events(start_date);
CREATE INDEX idx_academic_events_course ON academic_events(course_id);
CREATE INDEX idx_academic_events_type ON academic_events(event_type);
CREATE INDEX idx_academic_events_created_by ON academic_events(created_by);

-- Notifications Indexes
CREATE INDEX idx_notifications_user ON notifications(user_id);
CREATE INDEX idx_notifications_read ON notifications(is_read);
CREATE INDEX idx_notifications_created_at ON notifications(created_at);
CREATE INDEX idx_notifications_course ON notifications(course_id);

-- File Documents Indexes
CREATE INDEX idx_file_documents_uploaded_by ON file_documents(uploaded_by);
CREATE INDEX idx_file_documents_course ON file_documents(course_id);
CREATE INDEX idx_file_documents_department ON file_documents(department_id);
CREATE INDEX idx_file_documents_type ON file_documents(file_type);

-- Activity Logs Indexes
CREATE INDEX idx_activity_logs_user ON activity_logs(user_id);
CREATE INDEX idx_activity_logs_type ON activity_logs(activity_type);
CREATE INDEX idx_activity_logs_created_at ON activity_logs(created_at);

-- ===================================================================
-- 10. ROW LEVEL SECURITY (RLS) POLICIES
-- ===================================================================

-- Enable RLS on all tables
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE departments ENABLE ROW LEVEL SECURITY;
ALTER TABLE courses ENABLE ROW LEVEL SECURITY;
ALTER TABLE course_enrollments ENABLE ROW LEVEL SECURITY;
ALTER TABLE assignments ENABLE ROW LEVEL SECURITY;
ALTER TABLE assignment_submissions ENABLE ROW LEVEL SECURITY;
ALTER TABLE attendance_sessions ENABLE ROW LEVEL SECURITY;
ALTER TABLE attendance_records ENABLE ROW LEVEL SECURITY;
ALTER TABLE academic_events ENABLE ROW LEVEL SECURITY;
ALTER TABLE event_reminders ENABLE ROW LEVEL SECURITY;
ALTER TABLE notifications ENABLE ROW LEVEL SECURITY;
ALTER TABLE file_documents ENABLE ROW LEVEL SECURITY;
ALTER TABLE file_access_logs ENABLE ROW LEVEL SECURITY;
ALTER TABLE activity_logs ENABLE ROW LEVEL SECURITY;
ALTER TABLE system_logs ENABLE ROW LEVEL SECURITY;
ALTER TABLE app_settings ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_settings ENABLE ROW LEVEL SECURITY;

-- User Profiles Policies
CREATE POLICY "Users can view their own profile" ON user_profiles FOR SELECT USING (auth.uid() = id);
CREATE POLICY "Users can update their own profile" ON user_profiles FOR UPDATE USING (auth.uid() = id);
CREATE POLICY "Admins can view all profiles" ON user_profiles FOR ALL USING (
    EXISTS (SELECT 1 FROM user_profiles WHERE id = auth.uid() AND role = 'admin')
);

-- Departments Policies
CREATE POLICY "All users can view departments" ON departments FOR SELECT TO authenticated USING (true);
CREATE POLICY "Only admins can modify departments" ON departments FOR ALL USING (
    EXISTS (SELECT 1 FROM user_profiles WHERE id = auth.uid() AND role = 'admin')
);

-- Courses Policies
CREATE POLICY "Users can view courses in their department" ON courses FOR SELECT TO authenticated USING (
    department_id IN (
        SELECT department_id FROM user_profiles WHERE id = auth.uid()
    ) OR
    EXISTS (SELECT 1 FROM user_profiles WHERE id = auth.uid() AND role = 'admin')
);

-- Notifications Policies
CREATE POLICY "Users can view their own notifications" ON notifications FOR SELECT USING (user_id = auth.uid());
CREATE POLICY "Users can update their own notifications" ON notifications FOR UPDATE USING (user_id = auth.uid());

-- File Documents Policies
CREATE POLICY "Users can view files based on visibility" ON file_documents FOR SELECT TO authenticated USING (
    visibility = 'public' OR
    (visibility = 'department' AND department_id IN (
        SELECT department_id FROM user_profiles WHERE id = auth.uid()
    )) OR
    (visibility = 'course' AND course_id IN (
        SELECT course_id FROM course_enrollments WHERE student_id = auth.uid()
        UNION
        SELECT id FROM courses WHERE teacher_id = auth.uid()
    )) OR
    uploaded_by = auth.uid() OR
    EXISTS (SELECT 1 FROM user_profiles WHERE id = auth.uid() AND role = 'admin')
);

-- ===================================================================
-- 11. FUNCTIONS AND TRIGGERS
-- ===================================================================

-- Function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Apply updated_at trigger to relevant tables
CREATE TRIGGER update_user_profiles_updated_at BEFORE UPDATE ON user_profiles FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_departments_updated_at BEFORE UPDATE ON departments FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_courses_updated_at BEFORE UPDATE ON courses FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_assignments_updated_at BEFORE UPDATE ON assignments FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_academic_events_updated_at BEFORE UPDATE ON academic_events FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_file_documents_updated_at BEFORE UPDATE ON file_documents FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_app_settings_updated_at BEFORE UPDATE ON app_settings FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_user_settings_updated_at BEFORE UPDATE ON user_settings FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Function to create activity log
CREATE OR REPLACE FUNCTION create_activity_log(
    p_user_id UUID,
    p_title VARCHAR(255),
    p_description TEXT,
    p_activity_type VARCHAR(50),
    p_table_name VARCHAR(100) DEFAULT NULL,
    p_record_id UUID DEFAULT NULL
)
RETURNS UUID AS $$
DECLARE
    log_id UUID;
BEGIN
    INSERT INTO activity_logs (user_id, title, description, activity_type, table_name, record_id)
    VALUES (p_user_id, p_title, p_description, p_activity_type, p_table_name, p_record_id)
    RETURNING id INTO log_id;
    
    RETURN log_id;
END;
$$ LANGUAGE plpgsql;

-- ===================================================================
-- 12. INITIAL DATA SETUP
-- ===================================================================

-- Insert default departments
INSERT INTO departments (id, name, code, description) VALUES
    (uuid_generate_v4(), 'Computer Science', 'CS', 'Computer Science and Engineering Department'),
    (uuid_generate_v4(), 'Information Technology', 'IT', 'Information Technology Department'),
    (uuid_generate_v4(), 'Electronics', 'ECE', 'Electronics and Communication Engineering'),
    (uuid_generate_v4(), 'Mechanical', 'ME', 'Mechanical Engineering Department'),
    (uuid_generate_v4(), 'Civil', 'CE', 'Civil Engineering Department'),
    (uuid_generate_v4(), 'Administration', 'ADMIN', 'Administrative Department');

-- Insert default application settings
INSERT INTO app_settings (key, value, description, data_type, is_public) VALUES
    ('app_name', 'Campus 360', 'Application name', 'string', true),
    ('app_version', '1.0.0', 'Application version', 'string', true),
    ('institution_name', 'Your Institution Name', 'Name of the educational institution', 'string', true),
    ('academic_year', '2025-2026', 'Current academic year', 'string', true),
    ('semester_start_date', '2025-09-01', 'Current semester start date', 'string', false),
    ('semester_end_date', '2025-12-20', 'Current semester end date', 'string', false),
    ('max_file_size_mb', '50', 'Maximum file upload size in MB', 'number', false),
    ('allowed_file_types', '["pdf","doc","docx","ppt","pptx","xls","xlsx","jpg","jpeg","png"]', 'Allowed file upload types', 'json', false),
    ('notification_retention_days', '30', 'Days to retain notifications', 'number', false),
    ('attendance_threshold', '75', 'Minimum attendance percentage required', 'number', false);

-- ===================================================================
-- END OF SCHEMA
-- ===================================================================

-- Grant necessary permissions
GRANT USAGE ON SCHEMA public TO anon, authenticated;
GRANT ALL ON ALL TABLES IN SCHEMA public TO anon, authenticated;
GRANT ALL ON ALL SEQUENCES IN SCHEMA public TO anon, authenticated;
GRANT ALL ON ALL FUNCTIONS IN SCHEMA public TO anon, authenticated;

-- Success message
SELECT 'Campus 360 database schema created successfully!' as status;