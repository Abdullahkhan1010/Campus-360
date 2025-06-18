-- Quick test to verify the user_profiles table exists and has the correct structure
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'user_profiles' 
ORDER BY ordinal_position;

-- Also check if there are any existing users
SELECT COUNT(*) as user_count FROM user_profiles;

-- Check table constraints
SELECT 
    constraint_name,
    constraint_type
FROM information_schema.table_constraints 
WHERE table_name = 'user_profiles';
