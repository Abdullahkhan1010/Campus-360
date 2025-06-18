-- IMMEDIATE DATABASE FIX REQUIRED
-- Copy and paste this into Supabase SQL Editor and run it

-- Remove the foreign key constraint that's forcing ID to NULL
ALTER TABLE user_profiles 
DROP CONSTRAINT IF EXISTS user_profiles_id_fkey;

-- Ensure ID column can accept custom values (not auto-generated)
ALTER TABLE user_profiles 
ALTER COLUMN id DROP DEFAULT;

-- Change to TEXT type for compatibility with Supabase Auth UUIDs
ALTER TABLE user_profiles 
ALTER COLUMN id TYPE TEXT USING id::TEXT;

-- Verify the fix worked
SELECT 'Database constraint fixed successfully!' as status;
