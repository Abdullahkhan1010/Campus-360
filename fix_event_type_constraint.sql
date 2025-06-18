-- Fix event_type constraint to allow proper values
-- This script fixes the check constraint issue for academic_events table

-- First, let's see what the current constraint allows
SELECT 
    conname as constraint_name,
    pg_get_constraintdef(oid) as constraint_definition
FROM pg_constraint 
WHERE conrelid = (SELECT oid FROM pg_class WHERE relname = 'academic_events')
AND conname LIKE '%event_type%';

-- Drop the existing constraint if it exists
ALTER TABLE academic_events DROP CONSTRAINT IF EXISTS academic_events_event_type_check;

-- Create a new constraint that allows all the event types we use
ALTER TABLE academic_events 
ADD CONSTRAINT academic_events_event_type_check 
CHECK (event_type IN (
    'Assignment', 'Exam', 'Quiz', 'Project', 'Notice', 'Result',
    'ClassScheduled', 'ClassCancelled', 'Holiday', 'Semester',
    'Registration', 'AdmissionDeadline', 'FeePayment', 'Workshop',
    'Seminar', 'Meeting', 'Conference', 'Other',
    -- Also allow lowercase versions for flexibility
    'assignment', 'exam', 'quiz', 'project', 'notice', 'result',
    'classscheduled', 'classcancelled', 'holiday', 'semester',
    'registration', 'admissiondeadline', 'feepayment', 'workshop',
    'seminar', 'meeting', 'conference', 'other'
));

-- Check if the constraint was created successfully
SELECT 
    conname as constraint_name,
    pg_get_constraintdef(oid) as constraint_definition
FROM pg_constraint 
WHERE conrelid = (SELECT oid FROM pg_class WHERE relname = 'academic_events')
AND conname = 'academic_events_event_type_check';
