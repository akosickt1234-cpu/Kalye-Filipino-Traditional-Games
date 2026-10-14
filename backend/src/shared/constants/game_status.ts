export const GAME_STATUS = {
  WAITING: "waiting",
  PLAYING: "playing",
  FINISHED: "finished"
} as const

export type GameStatus = (typeof GAME_STATUS)[keyof typeof GAME_STATUS]
