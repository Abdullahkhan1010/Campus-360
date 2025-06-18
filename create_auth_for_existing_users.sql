-- Create auth users for existing user profiles
-- Run this in Supabase SQL Editor
-- WARNING: This will create auth users with temporary passwords

-- First, let's create a function to generate auth users for existing profiles
CREATE OR REPLACE FUNCTION create_auth_for_existing_users()
RETURNS TABLE(
    profile_id UUID,
    email TEXT,
    full_name TEXT,
    auth_created BOOLEAN,
    error_message TEXT
) AS $$
DECLARE
    user_record RECORD;
    temp_password TEXT := 'TempPassword123!';
    auth_user_id UUID;
BEGIN
    -- Loop through users that don't have auth users
    FOR user_record IN 
        SELECT up.id, up.email, up.full_name, up.role
        FROM user_profiles up
        LEFT JOIN auth.users au ON up.id = au.id
        WHERE au.id IS NULL
    LOOP
        BEGIN
            -- Generate a new UUID for the auth user
            auth_user_id := gen_random_uuid();
            
            -- Insert into auth.users (this is a simplified approach)
            -- Note: In production, you should use Supabase Auth API
            INSERT INTO auth.users (
                instance_id,
                id,
                aud,
                role,
                email,
                encrypted_password,
                email_confirmed_at,
                created_at,
                updated_at,
                raw_app_meta_data,
                raw_user_meta_data,
                is_super_admin,
                confirmation_token,
                email_change_token_new,
                recovery_token
            ) VALUES (
                '00000000-0000-0000-0000-000000000000',
                auth_user_id,
                'authenticated',
                'authenticated',
                user_record.email,
                crypt(temp_password, gen_salt('bf')), -- Simple password hashing
                NOW(),
                NOW(),
                NOW(),
                '{"provider":"email","providers":["email"]}',
                '{}',
                false,
                '',
                '',
                ''
            );
            
            -- Update the user profile to use the new auth user ID
            UPDATE user_profiles 
            SET id = auth_user_id 
            WHERE id = user_record.id;
            
            -- Return success
            profile_id := user_record.id;
            email := user_record.email;
            full_name := user_record.full_name;
            auth_created := true;
            error_message := NULL;
            RETURN NEXT;
            
        EXCEPTION WHEN OTHERS THEN
            -- Return error
            profile_id := user_record.id;
            email := user_record.email;
            full_name := user_record.full_name;
            auth_created := false;
            error_message := SQLERRM;
            RETURN NEXT;
        END;
    END LOOP;
    
    RETURN;
END;
$$ LANGUAGE plpgsql;

-- Execute the function (commented out for safety)
-- SELECT * FROM create_auth_for_existing_users();

-- Instead, let's do a safer approach:
-- Show what users would be affected
SELECT 
    up.id,
    up.email,
    up.full_name,
    up.role,
    'Would create auth user' as action
FROM user_profiles up
LEFT JOIN auth.users au ON up.id = au.id
WHERE au.id IS NULL;

SELECT 'Analysis complete. Uncomment the function call above to create auth users.' as status;
