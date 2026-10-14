import { z } from "zod"

export const code_param_schema = z.object({
  code: z.string().min(1).max(32)
})

export const send_body_schema = z.object({
  text: z.string()
})

export const poll_query_schema = z.object({
  after: z.coerce.number().int().min(0).optional(),
  wait: z.coerce.number().min(0).optional()
})
