-- =====================================================
-- QUICK FIX: REMOVE FOREIGN KEY CONSTRAINT ONLY
-- =====================================================
-- Simple script to just remove the foreign key constraint
-- that's preventing user creation

-- Step 1: Remove the foreign key constraint
ALTER TABLE user_profiles 
DROP CONSTRAINT IF EXISTS user_profiles_id_fkey;

-- Step 2: Remove any other foreign key constraints on user_profiles
DO $$
DECLARE
    constraint_rec RECORD;
BEGIN
    FOR constraint_rec IN 
        SELECT constraint_name 
        FROM information_schema.table_constraints 
        WHERE table_name = 'user_profiles' 
        AND constraint_type = 'FOREIGN KEY'
    LOOP
        EXECUTE 'ALTER TABLE user_profiles DROP CONSTRAINT IF EXISTS ' || constraint_rec.constraint_name;
        RAISE NOTICE 'Dropped constraint: %', constraint_rec.constraint_name;
    END LOOP;
END $$;

-- Step 3: Verify no foreign key constraints remain
SELECT 
    'All foreign key constraints removed from user_profiles' as status,
    COUNT(*) as remaining_foreign_keys
FROM information_schema.table_constraints 
WHERE constraint_type = 'FOREIGN KEY' 
    AND table_name = 'user_profiles';

-- Step 4: Show current table structure
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'user_profiles' 
    AND column_name = 'id';
