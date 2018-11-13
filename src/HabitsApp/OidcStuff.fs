namespace HabitsApp

open FSharp.Control.Tasks.ContextInsensitive
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication.OpenIdConnect
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open Saturn.Application
open System
open System.IdentityModel.Tokens.Jwt
open System.Text.Encodings.Web
open System.Threading.Tasks


module OidcStuff =

  let signOut : Giraffe.Core.HttpHandler = fun next (ctx: HttpContext) ->
    task {
      let! _ = ctx.SignOutAsync ()
      return! next ctx
    }

  type SslTerminationFriendlyOidcHandler (options: IOptionsMonitor<OpenIdConnectOptions>, logger: ILoggerFactory, htmlEncoder: HtmlEncoder, urlEncoder: UrlEncoder, clock: ISystemClock) =
    inherit OpenIdConnectHandler(options, logger, htmlEncoder, urlEncoder, clock)
    static member private X_FORWARDED_PROTO = "X-Forwarded-Proto"
    override this.HandleChallengeAsync (properties: Microsoft.AspNetCore.Authentication.AuthenticationProperties) =
      if this.Request.Headers.[SslTerminationFriendlyOidcHandler.X_FORWARDED_PROTO].Count > 0 then
        this.Request.Scheme <- this.Request.Headers.[SslTerminationFriendlyOidcHandler.X_FORWARDED_PROTO].[0]
      base.HandleChallengeAsync properties

  type AuthenticationBuilder with
    /// Based on: https://github.com/aspnet/Security/blob/cb83e4f48502057176b62d53e98de1d43db44a33/src/Microsoft.AspNetCore.Authentication.OpenIdConnect/OpenIdConnectExtensions.cs#L23-L27
    member builder.AddOpenIdConnect_SslTerminationFriendly (configureOptions: OpenIdConnectOptions -> unit) =
      builder.Services.TryAddEnumerable (ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIdConnectOptions>, OpenIdConnectPostConfigureOptions>())
      builder.AddRemoteScheme<OpenIdConnectOptions, SslTerminationFriendlyOidcHandler> (OpenIdConnectDefaults.AuthenticationScheme, OpenIdConnectDefaults.DisplayName, new Action<OpenIdConnectOptions>(configureOptions))

    member builder.EnsureCookieAdded (state: ApplicationState) =
      if not state.CookiesAlreadyAdded then builder.AddCookie() else builder

  type ApplicationBuilder with
    [<CustomOperation("use_oidc")>]
    member __.UseOidc (state: ApplicationState, clientId: string, clientSecret: string, authority: string) =
      let appConfig (app: IApplicationBuilder) = app.UseAuthentication()

      let serviceConfig (services: IServiceCollection) =
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear ()
        services
          .AddAuthentication(
            fun options ->
              options.DefaultAuthenticateScheme <- CookieAuthenticationDefaults.AuthenticationScheme
              options.DefaultSignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme
              options.DefaultChallengeScheme <- OpenIdConnectDefaults.AuthenticationScheme
          )
          .EnsureCookieAdded(state)
          .AddOpenIdConnect_SslTerminationFriendly (fun options ->
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
          )
        |> ignore

        services

      { state with
          ServicesConfig = serviceConfig::state.ServicesConfig
          AppConfigs = appConfig::state.AppConfigs
          CookiesAlreadyAdded = true
      }
