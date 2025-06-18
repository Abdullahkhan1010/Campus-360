-- ========================================
-- CREATE SAMPLE DEPARTMENTS
-- ========================================
-- This script creates sample departments for testing
-- Run this AFTER running FIX_ALL_RLS_COMPLETE.sql

-- First, check if departments exist
SELECT 'Current departments count:' as info;
SELECT COUNT(*) as department_count FROM departments;

-- Create sample departments if none exist
INSERT INTO departments (id, name, code, description, is_active, created_at, updated_at)
VALUES 
    ('dept_cs', 'Computer Science', 'CS', 'Department of Computer Science and Technology', true, NOW(), NOW()),
    ('dept_math', 'Mathematics', 'MATH', 'Department of Mathematics and Statistics', true, NOW(), NOW()),
    ('dept_eng', 'Engineering', 'ENG', 'Department of Engineering', true, NOW(), NOW()),
    ('dept_bus', 'Business Administration', 'BUS', 'Department of Business and Management', true, NOW(), NOW()),
    ('dept_sci', 'Natural Sciences', 'SCI', 'Department of Natural Sciences', true, NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- Verify departments were created
SELECT 'After inserting sample departments:' as info;
SELECT COUNT(*) as department_count FROM departments;

-- Show all departments
SELECT 'All departments:' as info;
SELECT id, name, code, description, is_active FROM departments ORDER BY name;
