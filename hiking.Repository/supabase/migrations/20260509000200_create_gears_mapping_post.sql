-- 3NF refactor: extract post-gear relationship into a junction table

-- Step 1: Create the mapping table
create table public.gears_mapping_post (
    id       uuid primary key default gen_random_uuid(),
    post_id  uuid not null references public.posts(id) on delete cascade,
    gear_id  uuid not null references public.gears(id) on delete cascade,
    weight   numeric not null default 0,
    quantity int     not null default 1
);

-- Step 2: Migrate existing data from gears into the mapping table
insert into public.gears_mapping_post (id, post_id, gear_id, weight, quantity)
select gen_random_uuid(), post_id, id, coalesce(weight, 0), coalesce(quantity, 1)
from public.gears;

-- Step 3: Remove only the relationship column from gears (weight/quantity remain as library defaults)
alter table public.gears
    drop column post_id;
