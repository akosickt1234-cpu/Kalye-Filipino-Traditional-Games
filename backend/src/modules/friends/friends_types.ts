export type PresenceStatus = "offline" | "online" | "in_lobby" | "in_game"

export type RelationStatus = "none" | "friends" | "request_sent" | "request_received"

export interface PublicUser {
  id: string
  username: string
  avatar: string
}

export interface FriendView extends PublicUser {
  status: PresenceStatus
}

export interface FriendRequestView extends PublicUser {
  created_at: string
}

export interface SearchResultView extends PublicUser {
  relation: RelationStatus
}

export interface InviteView {
  id: string
  from: PublicUser
  lobby_code: string
  created_at: string
  expires_at: string
}
