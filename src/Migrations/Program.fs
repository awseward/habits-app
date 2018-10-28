module Program

open System
open System.Reflection
open SimpleMigrations
open SimpleMigrations.DatabaseProvider
open SimpleMigrations.Console
open Npgsql

let private DATABASE_URL = Environment.GetEnvironmentVariable "DATABASE_URL"

let private _connectionString =
  let uri = (Uri DATABASE_URL)
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

[<EntryPoint>]
let main argv =
    let assembly = Assembly.GetExecutingAssembly()
    use db = new NpgsqlConnection (_connectionString)
    let provider = PostgresqlDatabaseProvider db
    let migrator = SimpleMigrator (assembly, provider)
    let consoleRunner = ConsoleRunner migrator
    consoleRunner.Run argv
    0
