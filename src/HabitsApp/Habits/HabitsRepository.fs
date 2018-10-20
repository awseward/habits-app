namespace Habits

open Database
open Microsoft.Data.Sqlite
open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive

module Database =
  let getAll connectionString : Task<Result<Habit seq, exn>> =
    task {
      use connection = new SqliteConnection(connectionString)
      return! query connection "SELECT id, name FROM Habits" None
    }

  let getById connectionString id : Task<Result<Habit option, exn>> =
    task {
      use connection = new SqliteConnection(connectionString)
      return! querySingle connection "SELECT id, name FROM Habits WHERE id=@id" (Some <| dict ["id" => id])
    }

  let update connectionString v : Task<Result<int,exn>> =
    task {
      use connection = new SqliteConnection(connectionString)
      return! execute connection "UPDATE Habits SET id = @id, name = @name WHERE id=@id" v
    }

  let insert connectionString v : Task<Result<int,exn>> =
    task {
      use connection = new SqliteConnection(connectionString)
      let query = @"
INSERT INTO Habits(id, name)
  VALUES (
    (SELECT IFNULL(MAX(id), 0) + 1 FROM Habits),
    @name
  )"
      return! execute connection query v
    }

  let delete connectionString id : Task<Result<int,exn>> =
    task {
      use connection = new SqliteConnection(connectionString)
      return! execute connection "DELETE FROM Habits WHERE id=@id" (dict ["id" => id])
    }

