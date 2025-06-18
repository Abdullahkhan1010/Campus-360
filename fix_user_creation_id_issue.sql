-- =================================================================
-- FIX USER CREATION ID ISSUE - Remove Foreign Key Constraint
-- =================================================================
-- This script removes the foreign key constraint that prevents
-- independent user profile creation
-- =================================================================

-- Step 1: Check current constraints
SELECT 
    constraint_name,
    table_name,
    constraint_type
FROM information_schema.table_constraints 
WHERE table_name = 'user_profiles' 
  AND constraint_type = 'FOREIGN KEY';

-- Step 2: Drop the foreign key constraint if it exists
DO $$ 
DECLARE
    constraint_rec RECORD;
BEGIN
    FOR constraint_rec IN 
        SELECT constraint_name 
        FROM information_schema.table_constraints 
        WHERE table_name = 'user_profiles' 
          AND constraint_type = 'FOREIGN KEY'
          AND constraint_name LIKE '%user_profiles_id_fkey%'
    LOOP
        EXECUTE 'ALTER TABLE user_profiles DROP CONSTRAINT IF EXISTS ' || constraint_rec.constraint_name;
        RAISE NOTICE 'Dropped constraint: %', constraint_rec.constraint_name;
    END LOOP;
END $$;

-- Step 3: Ensure the id column is properly configured as UUID with default
ALTER TABLE user_profiles 
ALTER COLUMN id SET DATA TYPE UUID USING id::UUID,
ALTER COLUMN id SET DEFAULT gen_random_uuid(),
ALTER COLUMN id SET NOT NULL;

-- Step 4: Verify the changes
SELECT 
    column_name, 
    data_type, 
    is_nullable, 
    column_default
FROM information_schema.columns 
WHERE table_name = 'user_profiles' 
  AND column_name = 'id';

-- Step 5: Test creating a user profile without auth dependency
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
    gen_random_uuid(),
    'test_independent@example.com',
    'Test Independent User',
    'student',
    NULL,
    false,
    true,
    NOW(),
    NOW()
) 
ON CONFLICT (email) DO NOTHING;

-- Step 6: Clean up test record
DELETE FROM user_profiles WHERE email = 'test_independent@example.com';

COMMIT;
