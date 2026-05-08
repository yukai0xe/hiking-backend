alter table public.gears
    add column if not exists category varchar(20) not null default '其他',
    add column if not exists quantity int not null default 1;
