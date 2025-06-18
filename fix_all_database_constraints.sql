-- COMPREHENSIVE DATABASE CONSTRAINT FIX
-- This script fixes all check constraints for the academic_events table
-- Run this in Supabase SQL Editor

-- First, let's see all current constraints
SELECT 
    conname as constraint_name,
    pg_get_constraintdef(oid) as constraint_definition
FROM pg_constraint 
WHERE conrelid = (SELECT oid FROM pg_class WHERE relname = 'academic_events')
AND contype = 'c'
ORDER BY conname;

-- Drop all existing check constraints that might cause issues
ALTER TABLE academic_events DROP CONSTRAINT IF EXISTS academic_events_event_type_check;
ALTER TABLE academic_events DROP CONSTRAINT IF EXISTS academic_events_status_check;
ALTER TABLE academic_events DROP CONSTRAINT IF EXISTS academic_events_priority_check;
ALTER TABLE academic_events DROP CONSTRAINT IF EXISTS academic_events_target_role_check;
ALTER TABLE academic_events DROP CONSTRAINT IF EXISTS academic_events_visibility_check;

-- Create new flexible constraints that allow both uppercase and lowercase

-- 1. Event Type Constraint
ALTER TABLE academic_events 
ADD CONSTRAINT academic_events_event_type_check 
CHECK (event_type IN (
    'Assignment', 'Exam', 'Quiz', 'Project', 'Notice', 'Result',
    'ClassScheduled', 'ClassCancelled', 'Holiday', 'Semester',
    'Registration', 'AdmissionDeadline', 'FeePayment', 'Workshop',
    'Seminar', 'Meeting', 'Conference', 'Other',
    'assignment', 'exam', 'quiz', 'project', 'notice', 'result',
    'classscheduled', 'classcancelled', 'holiday', 'semester',
    'registration', 'admissiondeadline', 'feepayment', 'workshop',
    'seminar', 'meeting', 'conference', 'other'
));

-- 2. Status Constraint
ALTER TABLE academic_events 
ADD CONSTRAINT academic_events_status_check 
CHECK (status IN (
    'Active', 'Draft', 'Cancelled', 'Completed', 'Postponed',
    'active', 'draft', 'cancelled', 'completed', 'postponed'
));

-- 3. Priority Constraint  
ALTER TABLE academic_events 
ADD CONSTRAINT academic_events_priority_check 
CHECK (priority IN (
    'Low', 'Normal', 'High', 'Critical',
    'low', 'normal', 'high', 'critical'
));

-- 4. Target Role Constraint
ALTER TABLE academic_events 
ADD CONSTRAINT academic_events_target_role_check 
CHECK (target_role IN (
    'All', 'Students', 'Teachers', 'Admin', 'Staff',
    'all', 'students', 'teachers', 'admin', 'staff'
));

-- Verify all constraints were created successfully
SELECT 
    conname as constraint_name,
    pg_get_constraintdef(oid) as constraint_definition
FROM pg_constraint 
WHERE conrelid = (SELECT oid FROM pg_class WHERE relname = 'academic_events')
AND contype = 'c'
AND conname LIKE '%check%'
ORDER BY conname;

-- Display success message
SELECT 'All database constraints have been updated successfully!' as status;
