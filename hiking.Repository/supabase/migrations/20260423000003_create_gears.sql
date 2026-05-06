create table public.gears (
  id      uuid primary key default gen_random_uuid(),
  post_id uuid not null references public.posts(id) on delete cascade,
  name    text not null,
  weight  numeric,
  note    text
);

alter table public.gears enable row level security;

create policy "public read gears"   on public.gears for select using (true);
create policy "public insert gears" on public.gears for insert with check (true);
