export const AVATAR_IDS = ["boy1", "girl1"] as const

export type AvatarId = (typeof AVATAR_IDS)[number]
