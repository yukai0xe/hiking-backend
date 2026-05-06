alter table public.posts
  add column if not exists weather      text,
  add column if not exists people_count int;
