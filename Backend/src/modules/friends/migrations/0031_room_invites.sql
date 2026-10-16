create table if not exists room_invites (
  id uuid primary key default gen_random_uuid(),
  from_user_id uuid not null references users(id) on delete cascade,
  to_user_id uuid not null references users(id) on delete cascade,
  lobby_code text not null,
  status text not null default 'pending' check (status in ('pending', 'accepted', 'declined')),
  created_at timestamptz not null default now(),
  expires_at timestamptz not null
);

create unique index if not exists room_invites_pending_unique
  on room_invites (from_user_id, to_user_id, lobby_code)
  where status = 'pending';

create index if not exists room_invites_to_user_idx on room_invites (to_user_id, status, expires_at);
create index if not exists room_invites_lobby_idx on room_invites (lobby_code);
create index if not exists room_invites_expires_idx on room_invites (expires_at);
