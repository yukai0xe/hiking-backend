alter table public.gears
    add column if not exists brand varchar(100),
    add column if not exists reference_url text,
    add column if not exists price int,
    add column if not exists added_at text;
