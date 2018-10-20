module Server

open System
open Saturn
open Config

let port =
  try
    UInt16.Parse (Environment.GetEnvironmentVariable "PORT")
  with
  | _ -> 8085us

let endpointPipe = pipeline {
    plug head
    plug requestId
}

let app = application {
    pipe_through endpointPipe

    error_handler (fun ex _ -> pipeline { set_status_code 500; render_html (InternalError.layout ex) })
    use_router Router.appRouter
    url (sprintf "http://0.0.0.0:%d/" port)
    memory_cache
    use_static "static"
    use_gzip
    use_config (fun _ -> { connectionString = "DataSource=database.sqlite" } ) //TODO: Set development time configuration
}

[<EntryPoint>]
let main _ =
    printfn "Working directory - %s" (System.IO.Directory.GetCurrentDirectory())
    run app
    0 // return an integer exit code
