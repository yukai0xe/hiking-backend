alter table public.gears
    add column if not exists deleted_at timestamptz;

drop policy if exists "public update gears" on public.gears;
create policy "public update gears" on public.gears
    for update using (true);

drop policy if exists "public delete gears" on public.gears;
create policy "public delete gears" on public.gears
    for delete using (true);
