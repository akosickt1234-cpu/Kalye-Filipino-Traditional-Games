import { Pool, type PoolClient, type QueryResult, type QueryResultRow } from "pg"
import { database_config } from "../../config/database_config"
import { child_logger } from "../../core/logging/logger"

const log = child_logger("database")

let pool: Pool | null = null

export function get_pool(): Pool {
  if (!pool) {
    pool = new Pool({
      connectionString: database_config.database_url,
      max: 10,
      connectionTimeoutMillis: 3000,
      idleTimeoutMillis: 30000
    })
    pool.on("error", (error) => log.error({ err: error }, "idle database client error"))
  }
  return pool
}

export async function query<T extends QueryResultRow = QueryResultRow>(
  text: string,
  params: unknown[] = []
): Promise<QueryResult<T>> {
  return get_pool().query<T>(text, params)
}

export async function with_transaction<T>(work: (client: PoolClient) => Promise<T>): Promise<T> {
  const client = await get_pool().connect()
  try {
    await client.query("begin")
    const result = await work(client)
    await client.query("commit")
    return result
  } catch (error) {
    await client.query("rollback").catch(() => undefined)
    throw error
  } finally {
    client.release()
  }
}

export async function check_database(): Promise<boolean> {
  try {
    await get_pool().query("select 1")
    return true
  } catch {
    return false
  }
}

export async function close_database(): Promise<void> {
  if (!pool) return
  const closing = pool
  pool = null
  await closing.end()
}
