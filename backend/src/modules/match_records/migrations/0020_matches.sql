create table if not exists matches (
  id uuid primary key,
  game_type text not null,
  lobby_code text,
  started_at timestamptz not null,
  ended_at timestamptz not null,
  created_at timestamptz not null default now()
);

create table if not exists match_players (
  match_id uuid not null references matches (id) on delete cascade,
  player_id text not null,
  user_id uuid,
  is_guest boolean not null default false,
  username text not null,
  team text,
  result text not null check (result in ('win', 'loss', 'draw')),
  score integer not null default 0,
  primary key (match_id, player_id)
);

create index if not exists match_players_user_id_idx on match_players (user_id, match_id);
create index if not exists matches_ended_at_idx on matches (ended_at desc, id desc);
create index if not exists matches_game_type_idx on matches (game_type);
