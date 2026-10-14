import { z } from "zod"

const user_id = z.string().uuid()

export const user_id_body_schema = z.object({ user_id })

export const user_id_param_schema = z.object({ user_id })

export const search_query_schema = z.object({
  q: z
    .string()
    .trim()
    .min(1)
    .max(20)
    .regex(/^[A-Za-z0-9_]+$/)
})

export const create_invite_schema = z.object({
  to_user_id: user_id,
  lobby_code: z
    .string()
    .trim()
    .min(1)
    .max(16)
    .regex(/^[A-Za-z0-9]+$/)
    .transform((value) => value.toUpperCase())
})

export const invite_id_param_schema = z.object({ id: z.string().uuid() })
