export interface RateLimiter {
  allow(key: string): boolean
  reset(key: string): void
  clear(): void
}

interface RateWindow {
  count: number
  reset_at: number
}

const PURGE_THRESHOLD = 1000

export function create_rate_limiter(limit: number, window_ms: number): RateLimiter {
  const windows = new Map<string, RateWindow>()

  function purge_expired(now: number): void {
    for (const [key, window] of windows) {
      if (window.reset_at <= now) windows.delete(key)
    }
  }

  return {
    allow(key) {
      const now = Date.now()
      if (windows.size > PURGE_THRESHOLD) purge_expired(now)
      const current = windows.get(key)
      if (!current || current.reset_at <= now) {
        windows.set(key, { count: 1, reset_at: now + window_ms })
        return limit >= 1
      }
      if (current.count >= limit) return false
      current.count += 1
      return true
    },
    reset(key) {
      windows.delete(key)
    },
    clear() {
      windows.clear()
    }
  }
}
