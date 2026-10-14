import type { ModuleDefinition } from "../../shared/types/module_types"
import { cors_policy } from "./cors_policy"
import { assert_log_redaction } from "./log_redaction_check"
import { global_rate_limit } from "./rate_limit"
import { request_id } from "./request_id"
import { security_headers } from "./security_headers"

const hardening_module: ModuleDefinition = {
  name: "hardening",
  middleware: [
    { order: 20, handler: request_id },
    { order: 22, handler: security_headers },
    { order: 24, handler: cors_policy },
    { order: 30, handler: global_rate_limit }
  ],
  on_start: async () => {
    assert_log_redaction()
  }
}

export default hardening_module
