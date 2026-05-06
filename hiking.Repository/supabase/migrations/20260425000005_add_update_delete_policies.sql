-- posts: allow update
create policy "public update posts"
  on public.posts for update
  using (true) with check (true);

-- posts: allow delete (for future use)
create policy "public delete posts"
  on public.posts for delete
  using (true);

-- photos: allow delete
create policy "public delete photos"
  on public.photos for delete
  using (true);

-- gears: allow update & delete
create policy "public update gears"
  on public.gears for update
  using (true) with check (true);

create policy "public delete gears"
  on public.gears for delete
  using (true);
