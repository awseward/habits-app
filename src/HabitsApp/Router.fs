module Router

open Saturn
open Giraffe.Core
open Giraffe.ResponseWriters
open System

type PipelineBuilder with
  [<CustomOperation("require_https")>]
  member __.RequireHttps (state, isRequired: bool) : HttpHandler =
    if isRequired then
      state
        >=> setHttpHeader "Strict-Transport-Security" "max-age=31536000; includeSubDomains"
        >=> fun next ctx ->
          let headerValues = ctx.Request.Headers.["X-Forwarded-Proto"]
          if headerValues.Count > 0 && headerValues.[0] = "http" then
            let path = if ctx.Request.Path.HasValue then ctx.Request.Path.Value else ""
            let builder = new System.UriBuilder ("https", ctx.Request.Host.Value, -1, path)
            let redirectUri = builder.Uri.GetComponents (UriComponents.Scheme ||| UriComponents.Host ||| UriComponents.PathAndQuery, UriFormat.SafeUnescaped)

            redirectTo true redirectUri next ctx
          else
            next ctx
    else state >=> fun next ctx -> next ctx

let browser = pipeline {
    require_https true

    plug acceptHtml
    plug putSecureBrowserHeaders
    plug fetchSession
    set_header "x-pipeline-type" "Browser"
}

let defaultView = router {
    get "/" (htmlView Index.layout)
    get "/index.html" (redirectTo false "/")
    get "/default.html" (redirectTo false "/")
    get "/github_oauth_callback" (redirectTo false "/habits")
}

let loggedInView = router {
  pipe_through User.loggedIn

  forward "" Habits.Controller.resource
}

let browserRouter = router {
    not_found_handler (setStatusCode 404 >=> htmlView NotFound.layout)
    pipe_through browser

    forward "" defaultView
    forward "/habits" loggedInView
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
