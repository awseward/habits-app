module Router

open Saturn
open Giraffe.Core
open Giraffe.ResponseWriters
open HabitsApp.HttpHandlers
open System

let browser = pipeline {
    require_https true

    plug acceptHtml
    plug putSecureBrowserHeaders
    plug fetchSession
    set_header "x-pipeline-type" "Browser"
}

let defaultView = router {
    get "/" (
      choose [
        userIsAuthenticated >=> redirectTo false "/habits"
        htmlView Index.layout
      ]
    )
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
