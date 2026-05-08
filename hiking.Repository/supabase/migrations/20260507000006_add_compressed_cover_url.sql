alter table public.posts
  add column if not exists compressed_cover_image text;
