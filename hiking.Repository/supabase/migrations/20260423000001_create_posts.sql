create table public.posts (
  id          uuid primary key default gen_random_uuid(),
  title       text not null,
  cover_image text,
  gpx_file    text,
  description text,
  created_at  timestamptz not null default now()
);

alter table public.posts enable row level security;

create policy "public read posts"   on public.posts for select using (true);
create policy "public insert posts" on public.posts for insert with check (true);
