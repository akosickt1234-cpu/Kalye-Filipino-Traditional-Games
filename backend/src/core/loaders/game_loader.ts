import { existsSync, readdirSync } from "node:fs"
import path from "node:path"
import type { GameModule } from "../../shared/types/game_types"
import { child_logger } from "../logging/logger"

const log = child_logger("game_loader")

const GAMES_ROOT = path.resolve(__dirname, "..", "..", "games")
const ENTRY_NAMES = ["index.ts", "index.js"]

function validate_game(folder: string, game: GameModule): void {
  if (!game || game.id !== folder) {
    throw new Error(`game in folder ${folder} must default export a GameModule with id "${folder}"`)
  }
  const { config } = game
  const valid_config =
    config &&
    typeof config.display_name === "string" &&
    config.display_name.length > 0 &&
    Number.isInteger(config.min_players) &&
    Number.isInteger(config.max_players) &&
    Number.isInteger(config.team_count) &&
    config.min_players >= 1 &&
    config.min_players <= config.max_players &&
    config.team_count >= 1
  if (!valid_config) {
    throw new Error(`game ${folder} has an invalid config`)
  }
}

class GameRegistry {
  private games = new Map<string, GameModule>()

  load(): GameModule[] {
    this.games.clear()
    if (!existsSync(GAMES_ROOT)) return []
    const folders = readdirSync(GAMES_ROOT, { withFileTypes: true })
      .filter((entry) => entry.isDirectory())
      .map((entry) => entry.name)
      .sort()
    for (const folder of folders) {
      const entry_file = ENTRY_NAMES.map((name) => path.join(GAMES_ROOT, folder, name)).find(existsSync)
      if (!entry_file) {
        log.warn({ folder }, "game folder has no index file, skipped")
        continue
      }
      const loaded = require(entry_file)
      const game: GameModule = loaded?.default ?? loaded
      validate_game(folder, game)
      this.games.set(game.id, game)
      log.info({ game: game.id }, "game loaded")
    }
    return this.list()
  }

  get(game_id: string): GameModule | undefined {
    return this.games.get(game_id)
  }

  list(): GameModule[] {
    return [...this.games.values()]
  }
}

export const game_registry = new GameRegistry()
