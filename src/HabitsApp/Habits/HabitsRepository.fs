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

  override __.SetValue (parameter, value) =
    match value with
    | Some v -> (Nullable<DateTimeOffset> v)
    | None -> Nullable<DateTimeOffset>()
    |> fun nullable -> parameter.Value <- nullable

module Database =
  let getAllForUserId connectionString userId : Task<Result<Habit seq, exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      let q = @"
SELECT id, name, last_done_at, user_id
FROM Habits
WHERE user_id = @user_id"
      return! query connection q (Some <| dict["user_id" => userId])
    }

  let getByUserIdAndId connectionString userId id : Task<Result<Habit option, exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      let q = @"
SELECT id, name, last_done_at
FROM Habits
WHERE
  user_id = @user_id
  AND id = @id"
      return! querySingle connection q (Some <| dict ["user_id" => userId; "id" => id])
    }

  let update connectionString v : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      let q = @"
UPDATE Habits SET
  name = @name,
  last_done_at = @last_done_at,
WHERE
  user_id = @user_id
  AND id = @id"
      return! execute connection q v
    }

  let insert connectionString v : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      let q = "INSERT INTO Habits (name, last_done_at, user_id) VALUES (@name, @last_done_at, @user_id)"
      return! execute connection q v
    }

  let delete connectionString id : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      let q = "DELETE FROM Habits WHERE id = @id"
      return! execute connection q (dict ["id" => id])
    }
