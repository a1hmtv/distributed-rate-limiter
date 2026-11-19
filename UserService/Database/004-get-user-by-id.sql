CREATE OR REPLACE FUNCTION get_user_by_id(id_p integer)
RETURNS SETOF public.users
LANGUAGE sql
AS $$
    SELECT * 
    FROM public.users
    WHERE id = id_p and is_deleted = false;
$$;
