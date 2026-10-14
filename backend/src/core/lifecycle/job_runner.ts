import { child_logger } from "../logging/logger"
import type { ModuleDefinition } from "../../shared/types/module_types"

const log = child_logger("jobs")

type JobDefinition = NonNullable<ModuleDefinition["jobs"]>[number]

interface RunningJob {
  key: string
  timer: NodeJS.Timeout
}

const running_jobs: RunningJob[] = []
const jobs_in_flight = new Set<Promise<void>>()

export function start_jobs(modules: ModuleDefinition[]): void {
  for (const module_definition of modules) {
    for (const job of module_definition.jobs ?? []) {
      schedule_job(`${module_definition.name}.${job.name}`, job)
    }
  }
}

function schedule_job(key: string, job: JobDefinition): void {
  let active = false
  const timer = setInterval(() => {
    if (active) return
    active = true
    const execution = job
      .run()
      .catch((error) => log.error({ err: error, job: key }, "job failed"))
      .finally(() => {
        active = false
        jobs_in_flight.delete(execution)
      })
    jobs_in_flight.add(execution)
  }, job.interval_ms)
  running_jobs.push({ key, timer })
  log.info({ job: key, interval_ms: job.interval_ms }, "job scheduled")
}

export async function stop_jobs(): Promise<void> {
  for (const job of running_jobs.splice(0)) {
    clearInterval(job.timer)
  }
  await Promise.allSettled([...jobs_in_flight])
}
