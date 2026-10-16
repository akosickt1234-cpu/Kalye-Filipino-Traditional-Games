create extension if not exists citext;

create table if not exists users (
  id uuid primary key default gen_random_uuid(),
  username citext not null unique,
  email citext not null unique,
  password_hash text not null,
  avatar text not null default 'boy1',
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);
