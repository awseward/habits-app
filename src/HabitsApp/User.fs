module User

open Saturn
open Giraffe
open System.Security.Claims

let matchUpUsers : HttpHandler = fun next ctx ->
  let isAdmin =
    false
    // A real implementation would match up user identities with something stored in a database

    // ctx.User.Claims
    // |> Seq.exists (fun claim -> claim.Issuer = "GitHub" && claim.Type = ClaimTypes.Name && claim.Value = "Admin Password")

  if isAdmin then
    ctx.User.AddIdentity (new ClaimsIdentity([Claim(ClaimTypes.Role, "Admin", ClaimValueTypes.String, "MyApplication")]))

  next ctx

let loggedIn = pipeline {
  requires_authentication (Giraffe.Auth.challenge "GitHub")
  plug matchUpUsers
}
