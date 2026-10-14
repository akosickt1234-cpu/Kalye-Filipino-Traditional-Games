import type { PlayerIdentity } from "./identity"

declare global {
  namespace Express {
    interface Request {
      player?: PlayerIdentity
    }
  }
}

export {}
