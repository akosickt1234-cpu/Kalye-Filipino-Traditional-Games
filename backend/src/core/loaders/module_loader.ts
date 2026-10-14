import { existsSync, readdirSync } from "node:fs"
import path from "node:path"
import type { ModuleDefinition } from "../../shared/types/module_types"
import { child_logger } from "../logging/logger"

const log = child_logger("module_loader")

const MODULES_ROOT = path.resolve(__dirname, "..", "..", "modules")
const ENTRY_NAMES = ["index.ts", "index.js"]

class ModuleRegistry {
  private modules: ModuleDefinition[] = []

  load(): ModuleDefinition[] {
    this.modules = []
    if (!existsSync(MODULES_ROOT)) return this.modules
    const folders = readdirSync(MODULES_ROOT, { withFileTypes: true })
      .filter((entry) => entry.isDirectory())
      .map((entry) => entry.name)
      .sort()
    for (const folder of folders) {
      const entry_file = ENTRY_NAMES.map((name) => path.join(MODULES_ROOT, folder, name)).find(existsSync)
      if (!entry_file) {
        log.warn({ folder }, "module folder has no index file, skipped")
        continue
      }
      const loaded = require(entry_file)
      const definition: ModuleDefinition | undefined = loaded?.default ?? loaded
      if (!definition || typeof definition.name !== "string" || definition.name.length === 0) {
        throw new Error(`module ${folder} must default export a ModuleDefinition with a name`)
      }
      if (this.modules.some((existing) => existing.name === definition.name)) {
        throw new Error(`duplicate module name ${definition.name}`)
      }
      this.modules.push(definition)
      log.info({ module: definition.name }, "module loaded")
    }
    return this.modules
  }

  list(): ModuleDefinition[] {
    return this.modules
  }
}

export const module_registry = new ModuleRegistry()
