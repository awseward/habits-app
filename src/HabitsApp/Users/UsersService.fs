namespace Users

open Config
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.ContextInsensitive
open Saturn


module Service =
  let getCurrentUserId (ctx: HttpContext) =
    ctx.Items.["user_id"] :?> int

  let setCurrentUserid (ctx: HttpContext) (id: int) =
    ctx.Items.Add ("user_id", id)
