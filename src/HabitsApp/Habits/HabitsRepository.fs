namespace Habits


open Database
open FSharp.Control.Tasks.ContextInsensitive
open Npgsql
open System.Threading.Tasks

module Database =
  let getAll connectionString : Task<Result<Habit seq, exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! query connection "SELECT id, name FROM Habits" None
    }

  let getById connectionString id : Task<Result<Habit option, exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! querySingle connection "SELECT id, name FROM Habits WHERE id=@id" (Some <| dict ["id" => id])
    }

  let update connectionString v : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection "UPDATE Habits SET id = @id, name = @name WHERE id=@id" v
    }

  let insert connectionString v : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      let query = "INSERT INTO Habits(name) VALUES (@name)"
      return! execute connection query v
    }

  let delete connectionString id : Task<Result<int,exn>> =
    task {
      use connection = new NpgsqlConnection(connectionString)
      return! execute connection "DELETE FROM Habits WHERE id=@id" (dict ["id" => id])
    }
