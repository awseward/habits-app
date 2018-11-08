module Router

open Saturn
open Giraffe.Core
open Giraffe.ResponseWriters
open HabitsApp.HttpHandlers
open System

let browser = pipeline {
    plug (requireHttps true)
    plug acceptHtml
    plug putSecureBrowserHeaders
    plug fetchSession
    set_header "x-pipeline-type" "Browser"
}

let loggedIn = pipeline {
  requires_authentication (Giraffe.Auth.challenge "GitHub")

  plug ensureUserPersisted
}

open Microsoft.AspNetCore.Authentication
open FSharp.Control.Tasks.ContextInsensitive
open System.Net.Http

let private _client =
  let client = new HttpClient ()
  client.DefaultRequestHeaders.Add ("User-Agent", "FIXME")
  client

let private _oauthClientId = Envars.get "GITHUB_OAUTH_CLIENT_ID"
let private _oauthClientSecret = Envars.get "GITHUB_OAUTH_CLIENT_SECRET"

let private _getBasicAuthHeaderValue () =
  _oauthClientSecret
  |> sprintf "%s:%s" _oauthClientId
  |> System.Text.ASCIIEncoding.ASCII.GetBytes
  |> Convert.ToBase64String
  |> sprintf "Basic %s"

let private _getAccessTokenRevokeUri =
  Uri << sprintf "https://api.github.com/applications/%s/tokens/%s" _oauthClientId

let revokeGithubToken : HttpHandler = fun next ctx ->
  task {
    let! token = ctx.GetTokenAsync "access_token"
    let uri = _getAccessTokenRevokeUri token
    let req = new HttpRequestMessage (HttpMethod.Delete, uri)
    req.Headers.Add ("Authorization", _getBasicAuthHeaderValue())
    use! response = _client.SendAsync (req, HttpCompletionOption.ResponseHeadersRead, ctx.RequestAborted)
    // response.EnsureSuccessStatusCode () |> ignore

    return! next ctx
  }

open Microsoft.AspNetCore.Authentication

let clearCookies : HttpHandler = fun next ctx ->
  task {
    let! _ = ctx.SignOutAsync ()
    return! next ctx
  }

let redirectToRoot : HttpHandler = redirectTo false "/"

let logOutView = router {
  get "" (choose [
            userIsAuthenticated >=> revokeGithubToken >=> clearCookies >=> redirectToRoot
            redirectToRoot
          ])
}

let defaultView = router {
    get "/" (choose [
                userIsAuthenticated >=> redirectTo false "/habits"
                htmlView Index.layout
              ])
    get "/github_oauth_callback" (redirectTo false "/habits")
}

let habitsView = router {
  pipe_through loggedIn

  forward "" Habits.Controller.resource
}

let browserRouter = router {
    not_found_handler (setStatusCode 404 >=> htmlView NotFound.layout)
    pipe_through browser

    forward "" defaultView
    forward "/habits" habitsView
    forward "/logout" logOutView
}

//Other scopes may use different pipelines and error handlers

// let api = pipeline {
//     plug acceptJson
//     set_header "x-pipeline-type" "Api"
// }

// let apiRouter = router {
//     pipe_through api

//     forward "/someApi" someScopeOrController
// }

let appRouter = router {
    // forward "/api" apiRouter
    forward "" browserRouter
}
