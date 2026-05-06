insert into storage.buckets (id, name, public)
values
  ('covers', 'covers', true),
  ('gpx',    'gpx',    true),
  ('photos', 'photos', true)
on conflict (id) do nothing;

create policy "public read covers"   on storage.objects for select using (bucket_id = 'covers');
create policy "public upload covers" on storage.objects for insert with check (bucket_id = 'covers');

create policy "public read gpx"      on storage.objects for select using (bucket_id = 'gpx');
create policy "public upload gpx"    on storage.objects for insert with check (bucket_id = 'gpx');

create policy "public read photos"   on storage.objects for select using (bucket_id = 'photos');
create policy "public upload photos" on storage.objects for insert with check (bucket_id = 'photos');
