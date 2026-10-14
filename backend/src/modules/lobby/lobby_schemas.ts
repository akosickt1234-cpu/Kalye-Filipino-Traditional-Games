import { z } from "zod"

const NO_CONTROL_CHARACTERS = /^[^\p{Cc}]+$/u

const lobby_name = z
  .string()
  .trim()
  .min(1)
  .max(40)
  .regex(NO_CONTROL_CHARACTERS, "name has invalid characters")

const game_type = z.string().trim().min(1).max(64)
const max_players = z.number().int()
const join_data = z.string().max(256)

export const create_lobby_schema = z.object({
  name: lobby_name,
  visibility: z.enum(["public", "private"]),
  game_type: game_type.optional(),
  max_players: max_players.optional(),
  join_data
})

export const update_lobby_schema = z
  .object({
    name: lobby_name.optional(),
    game_type: game_type.optional(),
    max_players: max_players.optional(),
    join_data: join_data.optional(),
    status: z.enum(["waiting", "in_game"]).optional()
  })
  .refine((value) => Object.values(value).some((field) => field !== undefined), {
    message: "at least one field is required"
  })

export const kick_schema = z.object({
  player_id: z.string().min(1).max(100)
})

export const heartbeat_query_schema = z.object({
  since: z.coerce.number().int().min(0).optional(),
  wait: z.coerce.number().min(0).optional()
})

export const result_schema = z.object({
  match_id: z.string().uuid(),
  started_at: z.string().datetime({ offset: true }),
  ended_at: z.string().datetime({ offset: true }),
  players: z
    .array(
      z.object({
        player_id: z.string().min(1).max(100),
        team: z.string().max(64).nullable().optional(),
        result: z.enum(["win", "loss", "draw"]),
        score: z.number().int()
      })
    )
    .min(1)
    .max(100)
})

export type CreateLobbyInput = z.infer<typeof create_lobby_schema>
export type UpdateLobbyInput = z.infer<typeof update_lobby_schema>
export type ResultInput = z.infer<typeof result_schema>
