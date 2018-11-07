module User

open Saturn
open Giraffe
open Config
open System.Security.Claims

open Users

let private _getConnectionString ctx =
  let cnf = Controller.getConfig ctx
  cnf.connectionString

let ensureUser : HttpHandler = fun next ctx ->
  let connectionString = _getConnectionString ctx
  let oauthType = ctx.User.Identity.AuthenticationType
  if (oauthType <> "GitHub") then invalidArg "Identity.AuthenticationType" "Value not supported"
  let oauthId =
    ctx.User.Claims
    |> Seq.find (fun claim -> claim.Type = "githubUsername")
    |> fun claim -> claim.Value
  let tryFindUser () = Users.Repository.getByOAuthInfo connectionString oauthType oauthId
  let addUserIdToItems (u: User) =
    ctx.Items.Add ("user_id", u.id)

  task {
    match! tryFindUser () with
    | Ok (Some found) -> addUserIdToItems found
    | Error ex -> raise ex
    | _ ->
        match! Users.Repository.insert connectionString { id = 0; oauth_type = oauthType; oauth_id = oauthId } with
        | Ok _ ->
            match! tryFindUser () with
            | Ok (Some inserted) -> addUserIdToItems inserted
            | Error ex -> raise ex
            | _ -> failwith "Unable to find just-inserted user."
        | Error ex -> raise ex

    return! (next ctx)
  }

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
  plug ensureUser
}
