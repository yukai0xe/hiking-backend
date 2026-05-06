alter table public.posts
  add column if not exists deleted_at timestamptz;
