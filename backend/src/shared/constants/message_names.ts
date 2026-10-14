export const MESSAGE_NAMES = {
  SELECT_AVATAR: "select_avatar",
  SET_READY: "set_ready",
  START_GAME: "start_game",
  UPDATE_SETTINGS: "update_settings",
  KICK_PLAYER: "kick_player",
  LEAVE_LOBBY: "leave_lobby",
  COUNTDOWN: "countdown",
  COUNTDOWN_CANCELLED: "countdown_cancelled",
  GAME_START: "game_start",
  RETURN_TO_LOBBY: "return_to_lobby",
  KICKED: "kicked",

  PLAYER_INPUT: "player_input",
  PLAYER_ACTION: "player_action",
  LEAVE_GAME: "leave_game",
  GAME_END: "game_end",

  CHAT_SEND: "chat_send",
  CHAT_MESSAGE: "chat_message",
  CHAT_HISTORY: "chat_history",

  FRIEND_ONLINE: "friend_online",
  FRIEND_OFFLINE: "friend_offline",
  FRIEND_STATUS: "friend_status",
  FRIEND_REQUEST_RECEIVED: "friend_request_received",
  FRIEND_REQUEST_ACCEPTED: "friend_request_accepted",
  INVITE_RECEIVED: "invite_received",

  ERROR: "error"
} as const

export type MessageName = (typeof MESSAGE_NAMES)[keyof typeof MESSAGE_NAMES]
