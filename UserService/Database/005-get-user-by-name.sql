CREATE OR REPLACE FUNCTION get_user_by_name(name_p text, surname_p text)
RETURNS SETOF public.users
LANGUAGE sql
AS $$
    SELECT *
    FROM public.users
    WHERE lower(name) = lower(name_p) and lower(surname) = lower(surname_p) 
        and is_deleted = false;
$$;
