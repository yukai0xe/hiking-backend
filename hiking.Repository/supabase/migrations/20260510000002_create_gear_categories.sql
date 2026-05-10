create table if not exists public.gear_categories (
    name text primary key
);

insert into public.gear_categories (name) values
    ('背負系統'), ('服裝'), ('營帳'), ('烹飪器具'),
    ('電子設備'), ('醫療用品'), ('其他')
on conflict do nothing;

alter table public.gear_categories enable row level security;

drop policy if exists "public read gear_categories" on public.gear_categories;
create policy "public read gear_categories" on public.gear_categories
    for select using (true);
