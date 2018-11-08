namespace HabitsApp

open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication.OpenIdConnect
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open Newtonsoft.Json.Linq
open Saturn.Application
open System.IdentityModel.Tokens.Jwt
open System.Text.Encodings.Web
open System.Threading.Tasks
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open FSharp.Control.Tasks.ContextInsensitive
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication.OpenIdConnect


module OidcStuff =

  let signOut : Giraffe.Core.HttpHandler = fun next (ctx: HttpContext) ->
    task {
      let! _ = ctx.SignOutAsync ()
      return! next ctx
    }

  type ApplicationBuilder with
    [<CustomOperation("use_oidc")>]
    member __.UseOidc (state: ApplicationState, clientId: string, clientSecret: string, authority: string) =
      let appConfig (app: IApplicationBuilder) = app.UseAuthentication ()

      let serviceConfig (services: IServiceCollection) =
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear ()
        services
          .AddAuthentication(
            fun options ->
              options.DefaultAuthenticateScheme <- CookieAuthenticationDefaults.AuthenticationScheme
              options.DefaultSignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme
              options.DefaultChallengeScheme <- OpenIdConnectDefaults.AuthenticationScheme
          )
          .AddCookie()
          .AddOpenIdConnect(
            fun options ->
              options.ClientId <- clientId
              options.ClientSecret <- clientSecret
              options.Authority <- authority

              options.ResponseType <- OpenIdConnectResponseType.CodeIdToken
              options.SaveTokens <- true
              options.GetClaimsFromUserInfoEndpoint <- true

              options.ClaimActions.MapAllExcept [|"aud"; "iss"; "iat"; "nbf"; "exp"; "aio"; "c_hash"; "uti"; "nonce"|]

              options.Events.OnAuthenticationFailed <-
                (fun ctx ->
                  task {
                    ctx.HandleResponse ()

                    ctx.Response.StatusCode <- 500
                    ctx.Response.ContentType <- "text/plain"
                    do! ctx.Response.WriteAsync "An error occurred processing your authentication."
                  }
                  :> Task
                )
          ) |> ignore
        services

      { state with
          ServicesConfig = serviceConfig::state.ServicesConfig
          AppConfigs = appConfig::state.AppConfigs
          ApplicationState.CookiesAlreadyAdded = false // Maybe, maybe not
      }
