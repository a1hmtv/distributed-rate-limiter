CREATE INDEX idx_name_surname
ON public.users(lower(name), lower(surname));