namespace Habits


open Database
open FSharp.Control.Tasks.ContextInsensitive
open Npgsql
open System.Threading.Tasks
open System
open Dapper

type NullableDateTimeOffsetHandler () =
  inherit SqlMapper.TypeHandler<DateTimeOffset option> ()

  override __.Parse value =
    printfn "Parse ()"
    printfn "%A" value
    match value with
    | null -> None
    | :? DateTime as dt ->
        eprintfn "WARNING: DateTime -> DateTimeOffset conversion likely to cause time zone errors"
        Some (DateTimeOffset dt)
    | :? DateTimeOffset as dto ->
        Some dto
    | x ->
        eprintfn "WARNING: Unsure how to convert object to DateTimeOffset: %A" x
        None


  override __.SetValue (dbDataParameter, value) =
    match value with
    | Some v -> (Nullable<DateTimeOffset> v)
    | None -> Nullable<DateTimeOffset>()
    |> fun nullable -> dbDataParameter.Value <- nullable

module Database =
  let getAll connectionString : Task<Result<Habit seq, exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! query connection "SELECT id, name, last_done_at FROM Habits" None
    }

  let getById connectionString id : Task<Result<Habit option, exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! querySingle connection "SELECT id, name, last_done_at FROM Habits WHERE id=@id" (Some <| dict ["id" => id])
    }

  let update connectionString v : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection "UPDATE Habits SET id = @id, name = @name, last_done_at = @last_done_at WHERE id=@id" v
    }

  let insert connectionString v : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      let query = "INSERT INTO Habits(name, last_done_at) VALUES (@name, NULL)"
      return! execute connection query v
    }

  let delete connectionString id : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection "DELETE FROM Habits WHERE id=@id" (dict ["id" => id])
    }
