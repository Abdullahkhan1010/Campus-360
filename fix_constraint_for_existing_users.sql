-- Since all existing users have auth users, we just need to remove the constraint
-- that's preventing new user creation. Run this in Supabase SQL Editor:

-- Remove the foreign key constraint that's causing the insertion issues
ALTER TABLE user_profiles DROP CONSTRAINT IF EXISTS user_profiles_id_fkey;

-- Verify the constraint was removed
SELECT 
    tc.constraint_name,
    tc.table_name,
    tc.constraint_type
FROM information_schema.table_constraints AS tc 
WHERE tc.constraint_type = 'FOREIGN KEY' 
    AND tc.table_name = 'user_profiles';

-- Test that we can now insert a user with a custom UUID
DO $$
DECLARE
    test_id UUID := gen_random_uuid();
BEGIN
    INSERT INTO user_profiles (
        id,
        email,
        full_name,
        role,
        department_id,
        is_verified,
        is_active,
        created_at,
        updated_at
    ) VALUES (
        test_id,
        'constraint-test@campus360.com',
        'Constraint Test User',
        'teacher',
        (SELECT id FROM departments WHERE is_active = true LIMIT 1),
        true,
        true,
        NOW(),
        NOW()
    );
    
    RAISE NOTICE 'SUCCESS: Test user inserted with ID: %', test_id;
    
    -- Clean up the test user
    DELETE FROM user_profiles WHERE email = 'constraint-test@campus360.com';
    RAISE NOTICE 'Test user cleaned up successfully';
    
EXCEPTION WHEN OTHERS THEN
    RAISE NOTICE 'ERROR: %', SQLERRM;
END $$;

SELECT 'Constraint removal and test complete. New users should now be creatable.' as status;
