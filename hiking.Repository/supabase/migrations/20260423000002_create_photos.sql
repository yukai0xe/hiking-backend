create table public.photos (
  id         uuid primary key default gen_random_uuid(),
  post_id    uuid not null references public.posts(id) on delete cascade,
  url        text not null,
  created_at timestamptz not null default now()
);

alter table public.photos enable row level security;

create policy "public read photos"   on public.photos for select using (true);
create policy "public insert photos" on public.photos for insert with check (true);
