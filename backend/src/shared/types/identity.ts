export interface PlayerIdentity {
  id: string
  username: string
  is_guest: boolean
  avatar: string
}

export interface AccessTokenPayload {
  sub: string
  username: string
  is_guest: boolean
  avatar: string
  iat: number
  exp: number
}
