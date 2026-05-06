create table if not exists public.tags (
  id         uuid primary key default gen_random_uuid(),
  name       text not null unique,
  created_at timestamptz default now()
);

alter table public.tags enable row level security;

create policy "Public read tags"   on public.tags for select using (true);
create policy "Public insert tags" on public.tags for insert with check (true);

insert into public.tags (name) values
  ('臺灣百岳'),
  ('臺灣小百岳'),
  ('中央山脈縱走'),
  ('谷關七雄'),
  ('北坑溪古道'),
  ('高山湖泊'),
  ('中級山縱走'),
  ('日本百名山'),
  ('4000 米以上高山'),
  ('郊山'),
  ('中級山'),
  ('臺中市大坑步道')
on conflict (name) do nothing;
