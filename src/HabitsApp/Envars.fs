module internal Envars

open System

module Option =
  let ofString (str: string) = if (String.IsNullOrWhiteSpace str) then None else Some str

let tryGet = Environment.GetEnvironmentVariable >> Option.ofString

let getEnv () =
  "ENV"
  |> tryGet
  |> Option.defaultValue "development"

let private _guessDevDbUrlIfNone dbUrlOpt =
  dbUrlOpt
  |> Option.defaultWith (fun () ->
      "USER"
      |> tryGet
      |> Option.map (fun user -> sprintf "http://%s:FIXME_PASS@localhost:5432/habits_app_dev" user)
      |> Option.defaultValue ""
  )

let getDatabaseUrl () =
  "DATABASE_URL"
  |> tryGet
  |> _guessDevDbUrlIfNone
