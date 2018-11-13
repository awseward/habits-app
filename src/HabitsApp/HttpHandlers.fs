namespace HabitsApp
open Saturn.Pipeline
open System.Threading.Tasks

module HttpHandlers =
  open Config
  open Giraffe.Core
  open System
  open Giraffe
  open Users

  let passThrough : HttpHandler = fun next ctx -> next ctx

  let setHsts : HttpHandler = setHttpHeader "Strict-Transport-Security" "max-age=31536000; includeSubDomains"

  let redirectHttps : HttpHandler =
    fun next ctx ->
      let headerValues = ctx.Request.Headers.["X-Forwarded-Proto"]
      if headerValues.Count > 0 && headerValues.[0] = "http" then
        let path = if ctx.Request.Path.HasValue then ctx.Request.Path.Value else ""
        let builder = new System.UriBuilder ("https", ctx.Request.Host.Value, -1, path)
        let redirectUri = builder.Uri.GetComponents (UriComponents.Scheme ||| UriComponents.Host ||| UriComponents.PathAndQuery, UriFormat.SafeUnescaped)
        redirectTo true redirectUri next ctx
      else
        next ctx

  let userIsAuthenticated : HttpHandler =
    fun next ctx ->
      if (isNull ctx.User || isNull ctx.User.Identity || not ctx.User.Identity.IsAuthenticated) then Task.FromResult None
      else
        next ctx

  let requireHttps isRequired =
    if isRequired then setHsts >=> redirectHttps
    else
      passThrough

  type PipelineBuilder with
    [<CustomOperation("require_https")>]
    member __.RequireHttps (state, isRequired) : HttpHandler = state >=> (requireHttps isRequired)

  let private _getConnectionString ctx =
    let config = Saturn.ControllerHelpers.Controller.getConfig ctx
    config.connectionString

  // FIXME: This is probably doing all kinds of wrong things
  let ensureUserPersisted : HttpHandler = fun next ctx ->
    let connectionString = _getConnectionString ctx
    let authType = ctx.User.Identity.AuthenticationType
    if (authType <> "AuthenticationTypes.Federation") then invalidArg "Identity.AuthenticationType" (sprintf "Value '%s' not supported" authType)
    let oauthId =
      ctx.User.Claims
      |> Seq.find (fun claim -> claim.Type = "sub")
      |> fun claim -> claim.Value
    let tryFindUser () = Users.Repository.getByOAuthInfo connectionString authType oauthId
    let addUserIdToItems (u: Users.User) = Users.Service.setCurrentUserid ctx u.id

    task {
      match! tryFindUser () with
      | Ok (Some found) -> addUserIdToItems found
      | Error ex -> raise ex
      | _ ->
          match! Users.Repository.insert connectionString { id = 0; oauth_type = authType; oauth_id = oauthId } with
          | Ok _ ->
              match! tryFindUser () with
              | Ok (Some inserted) -> addUserIdToItems inserted
              | Ok None -> failwith "Unable to find just-inserted user."
              | Error ex -> raise ex
          | Error ex -> raise ex

      return! (next ctx)
    }
