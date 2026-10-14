import { existsSync, readdirSync, readFileSync } from "node:fs"
import path from "node:path"
import { child_logger } from "../../core/logging/logger"
import { close_database, with_transaction } from "./database_client"

const log = child_logger("migrations")

const SOURCE_ROOT = path.resolve(__dirname, "..", "..", "..", "src")
const MIGRATION_LOCK_ID = 727001

interface MigrationFile {
  name: string
  file_path: string
}

function list_sql_files(directory: string): MigrationFile[] {
  if (!existsSync(directory)) return []
  return readdirSync(directory)
    .filter((file_name) => file_name.endsWith(".sql"))
    .map((file_name) => ({ name: file_name, file_path: path.join(directory, file_name) }))
}

export function collect_migrations(): MigrationFile[] {
  const directories = [path.join(SOURCE_ROOT, "infrastructure", "database", "migrations")]
  const modules_directory = path.join(SOURCE_ROOT, "modules")
  if (existsSync(modules_directory)) {
    for (const entry of readdirSync(modules_directory, { withFileTypes: true })) {
      if (entry.isDirectory()) {
        directories.push(path.join(modules_directory, entry.name, "migrations"))
      }
    }
  }
  const files = directories.flatMap(list_sql_files)
  const seen = new Map<string, string>()
  for (const file of files) {
    const existing = seen.get(file.name)
    if (existing) {
      throw new Error(`duplicate migration name ${file.name}: ${existing} and ${file.file_path}`)
    }
    seen.set(file.name, file.file_path)
  }
  return files.sort((a, b) => a.name.localeCompare(b.name))
}

export async function run_migrations(): Promise<string[]> {
  const migrations = collect_migrations()
  return with_transaction(async (client) => {
    await client.query("select pg_advisory_xact_lock($1)", [MIGRATION_LOCK_ID])
    await client.query(
      `create table if not exists schema_migrations (
        name text primary key,
        applied_at timestamptz not null default now()
      )`
    )
    const applied_result = await client.query<{ name: string }>("select name from schema_migrations")
    const applied = new Set(applied_result.rows.map((row) => row.name))
    const newly_applied: string[] = []
    for (const migration of migrations) {
      if (applied.has(migration.name)) continue
      const sql = readFileSync(migration.file_path, "utf8")
      await client.query(sql)
      await client.query("insert into schema_migrations (name) values ($1)", [migration.name])
      newly_applied.push(migration.name)
      log.info({ migration: migration.name }, "migration applied")
    }
    if (newly_applied.length === 0) log.info("database is up to date")
    return newly_applied
  })
}

if (require.main === module) {
  run_migrations()
    .then(() => close_database())
    .then(() => process.exit(0))
    .catch(async (error) => {
      log.error({ err: error }, "migration failed")
      await close_database().catch(() => undefined)
      process.exit(1)
    })
}
