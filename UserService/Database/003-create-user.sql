CREATE OR REPLACE function create_user(login_p text, password_p text, name_p text, surname_p text, age_p integer)
RETURNS INTEGER
LANGUAGE sql
AS $$
    INSERT INTO public.users (login, password, name, surname, age) 
    VALUES (login_p, password_p, name_p, surname_p, age_p)
    RETURNING id;
$$;
