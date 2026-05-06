alter table public.posts
  add column if not exists date_start   text,
  add column if not exists date_end     text,
  add column if not exists weather      text,
  add column if not exists people_count int,
  add column if not exists deleted_at   timestamptz;
