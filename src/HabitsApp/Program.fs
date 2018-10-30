module Server

open System
open Saturn
open Config
open Habits

let port =
  try
    UInt16.Parse (Environment.GetEnvironmentVariable "PORT")
  with
  | _ -> 8085us

let private _connectionString =
  let uri = Uri <| Envars.getDatabaseUrl()

  let username, password =
    match uri.UserInfo.Split (':', StringSplitOptions.RemoveEmptyEntries) with
    | [|user; pass|] -> user, pass
    | _ -> failwith "Database url should contain user and pass, but didn't."
  let host = uri.Host
  let port = uri.Port
  let database = uri.AbsolutePath.TrimStart '/'

  sprintf
    "Host=%s;Port=%i;Username=%s;Password=%s;Database=%s"
    host
    port
    username
    password
    database

let private _isProduction = ("production" = Environment.GetEnvironmentVariable "ENV")

let endpointPipe = pipeline {
    plug head
    plug requestId
}

let app = application {
    pipe_through endpointPipe

    error_handler (fun ex _ -> pipeline { set_status_code 500; render_html (InternalError.layout _isProduction ex) })
    use_router Router.appRouter
    url (sprintf "http://0.0.0.0:%d/" port)
    memory_cache
    use_static "static"
    use_gzip
    use_config (fun _ -> { connectionString = _connectionString })
}

[<EntryPoint>]
let main _ =
    printfn "Working directory - %s" (System.IO.Directory.GetCurrentDirectory())
    Dapper.SqlMapper.AddTypeHandler (NullableDateTimeOffsetHandler())
    run app
    0
