import { environment } from "./environment"

export const database_config = {
  database_url: environment.DATABASE_URL
} as const
