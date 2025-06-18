-- Check the event_type constraint to see what values are allowed
SELECT 
    conname as constraint_name,
    pg_get_constraintdef(oid) as constraint_definition
FROM pg_constraint 
WHERE conname LIKE '%event_type%' 
AND conrelid = 'academic_events'::regclass;

-- Also check the table structure to understand the event_type column
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'academic_events' 
AND column_name = 'event_type'
ORDER BY ordinal_position;

-- Check if there are any enum types for event_type
SELECT 
    t.typname,
    e.enumlabel
FROM pg_type t 
JOIN pg_enum e ON t.oid = e.enumtypid  
WHERE t.typname LIKE '%event%'
ORDER BY t.typname, e.enumsortorder;
