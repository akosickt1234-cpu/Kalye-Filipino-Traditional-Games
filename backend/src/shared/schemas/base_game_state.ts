import { MapSchema, Schema, type } from "@colyseus/schema"
import { GAME_STATUS } from "../constants/game_status"

export class BaseGamePlayer extends Schema {
  @type("string") id = ""
  @type("string") username = ""
  @type("string") team = ""
  @type("number") score = 0
  @type("boolean") connected = true
  @type("number") x = 0
  @type("number") y = 0
}

export class BaseGameState extends Schema {
  @type("string") status: string = GAME_STATUS.WAITING
  @type("uint32") tick = 0
  @type("number") time_left = 0
  @type({ map: BaseGamePlayer }) players = new MapSchema<BaseGamePlayer>()
}
