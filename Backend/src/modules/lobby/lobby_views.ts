import type {
  Lobby,
  LobbyMemberView,
  LobbySummaryView,
  LobbyView,
  PublicLobbyView
} from "./lobby_types"

export function lobby_view(lobby: Lobby): LobbyView {
  return {
    code: lobby.code,
    version: lobby.version,
    name: lobby.name,
    visibility: lobby.visibility,
    game_type: lobby.game_type,
    host_id: lobby.host_id,
    status: lobby.status,
    max_players: lobby.max_players,
    players: [...lobby.members.values()].map((member) => ({
      id: member.id,
      username: member.username,
      avatar: member.avatar,
      is_guest: member.is_guest,
      is_host: member.id === lobby.host_id
    }))
  }
}

export function member_view(lobby: Lobby): LobbyMemberView {
  return { ...lobby_view(lobby), join_data: lobby.join_data }
}

export function public_view(lobby: Lobby): PublicLobbyView {
  return {
    code: lobby.code,
    name: lobby.name,
    host_username: lobby.members.get(lobby.host_id)?.username ?? "",
    game_type: lobby.game_type,
    player_count: lobby.members.size,
    max_players: lobby.max_players
  }
}

export function summary_view(lobby: Lobby): LobbySummaryView {
  return {
    code: lobby.code,
    name: lobby.name,
    status: lobby.status,
    player_count: lobby.members.size,
    max_players: lobby.max_players
  }
}
