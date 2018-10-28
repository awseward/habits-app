module Server

open System
open Saturn
open Config

let port =
  try
    UInt16.Parse (Environment.GetEnvironmentVariable "PORT")
  with
  | _ -> 8085us

let private _connectionString =
  "DataSource=database.sqlite" // FIXME

let endpointPipe = pipeline {
    plug head
    plug requestId
}

let app = application {
    pipe_through endpointPipe

    error_handler (fun ex _ -> pipeline { render_html (InternalError.layout ex) })
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
    run app
    0
