import { z } from "zod"
import { AVATAR_IDS } from "../../shared/constants/avatar_ids"

const USERNAME_PATTERN = /^[A-Za-z0-9_]{3,20}$/

const username_schema = z
  .string()
  .regex(USERNAME_PATTERN, "username must be 3 to 20 characters: letters, digits, underscore")

export const register_schema = z.object({
  username: username_schema,
  email: z.string().trim().max(254).email(),
  password: z.string().min(8).max(128)
})

export const login_schema = z.object({
  identifier: z.string().trim().min(1).max(254),
  password: z.string().min(1).max(128)
})

export const guest_schema = z.object({
  username: z.string().max(100).optional(),
  avatar: z.enum(AVATAR_IDS).optional()
})

export const refresh_schema = z.object({
  refresh_token: z.string().min(1).max(256)
})

export const update_me_schema = z
  .object({
    username: username_schema.optional(),
    avatar: z.enum(AVATAR_IDS).optional()
  })
  .refine((value) => value.username !== undefined || value.avatar !== undefined, {
    message: "provide username or avatar"
  })

export type RegisterInput = z.infer<typeof register_schema>
export type LoginInput = z.infer<typeof login_schema>
export type GuestInput = z.infer<typeof guest_schema>
export type UpdateMeInput = z.infer<typeof update_me_schema>
