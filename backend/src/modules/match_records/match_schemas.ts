import { z } from "zod"

export const list_matches_query = z.object({
  limit: z.coerce.number().int().min(1).max(50).default(20),
  cursor: z.string().min(1).max(200).optional()
})

export const leaderboard_params = z.object({
  game_type: z.string().min(1).max(64)
})

export const leaderboard_query = z.object({
  limit: z.coerce.number().int().min(1).max(100).default(20)
})
