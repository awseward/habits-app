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

open Microsoft.AspNetCore.Authentication.OpenIdConnect

let loggedIn = pipeline {
  requires_authentication (Giraffe.Auth.challenge OpenIdConnectDefaults.AuthenticationScheme)

  plug ensureUserPersisted
}

open System.Net.Http

let redirectToRoot : HttpHandler = redirectTo false "/"

let logOutView = router {
  get "" (choose [
            userIsAuthenticated >=> HabitsApp.OidcStuff.signOut >=> redirectToRoot
            redirectToRoot
          ])
}

let defaultView = router {
    get "/" (choose [
                userIsAuthenticated >=> redirectTo false "/habits"
                htmlView Index.layout
              ])
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
