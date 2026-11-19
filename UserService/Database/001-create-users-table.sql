CREATE TABLE public.users(
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    login text NOT NULL UNIQUE,
    password text NOT NULL,
    name text NOT NULL,
    surname text NOT NULL,
    age integer NOT NULL CHECK ( age > 0 ),
    is_deleted boolean DEFAULT FALSE
)