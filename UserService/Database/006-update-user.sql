CREATE OR REPLACE FUNCTION update_user(
    id_p integer, 
    password_p text DEFAULT NULL,
    name_p text DEFAULT NULL, 
    surname_p text DEFAULT NULL,
    age_p integer DEFAULT NULL)
RETURNS SETOF public.users
LANGUAGE sql
AS $$
    UPDATE public.users
    SET password = COALESCE(password_p, password),
        name = COALESCE(name_p, name),
        surname = COALESCE(surname_p, surname),
        age = COALESCE(age_p, age)
    WHERE id = id_p and is_deleted = false
    RETURNING *;
$$;
