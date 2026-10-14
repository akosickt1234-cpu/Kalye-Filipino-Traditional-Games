import { Room, ServerError, type AuthContext, type Client } from "@colyseus/core"
import { ERROR_CODES, type ErrorCode } from "../../shared/constants/error_codes"
import { MESSAGE_NAMES } from "../../shared/constants/message_names"
import type { PlayerIdentity } from "../../shared/types/identity"
import { payload_to_identity, verify_access_token } from "../../shared/utils/token_utils"
import { default_message_for_code, is_app_error } from "../errors/app_error"
import { module_registry } from "../loaders/module_loader"

export abstract class BaseRoom<State extends object = any, Metadata = any> extends Room<
  State,
  Metadata,
  any,
  PlayerIdentity
> {
  async onAuth(client: Client<any, PlayerIdentity>, options: any, _context?: AuthContext): Promise<PlayerIdentity> {
    const token = options?.token
    if (typeof token !== "string" || token.length === 0) {
      throw new ServerError(401, ERROR_CODES.UNAUTHENTICATED)
    }
    let identity: PlayerIdentity
    try {
      identity = payload_to_identity(verify_access_token(token))
    } catch (error) {
      const code = is_app_error(error) ? error.code : ERROR_CODES.INVALID_TOKEN
      throw new ServerError(401, code)
    }
    await this.authorize_identity(identity, options)
    return identity
  }

  protected async authorize_identity(_identity: PlayerIdentity, _options: any): Promise<void> {}

  onCreate(_options: any): void | Promise<any> {
    module_registry.attach_room_features(this)
  }

  send_error(client: Client, code: ErrorCode, message?: string): void {
    client.send(MESSAGE_NAMES.ERROR, { code, message: message ?? default_message_for_code(code) })
  }

  get_identity(client: Client): PlayerIdentity {
    return client.auth as PlayerIdentity
  }
}
