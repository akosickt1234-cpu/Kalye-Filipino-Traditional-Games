export const ROOM_NAMES = {
  LOBBY: "lobby_room",
  GAME: "game_room",
  PRESENCE: "presence_room"
} as const

export type RoomName = (typeof ROOM_NAMES)[keyof typeof ROOM_NAMES]
