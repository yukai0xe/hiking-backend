alter table public.posts
  add column if not exists date_start text,
  add column if not exists date_end   text;
