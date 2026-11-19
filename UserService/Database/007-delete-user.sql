CREATE OR REPLACE FUNCTION delete_user(id_p integer)
RETURNS BOOLEAN 
LANGUAGE sql
AS $$
UPDATE public.users
    SET is_deleted = true
    WHERE id = id_p and is_deleted = false
    RETURNING is_deleted
$$;