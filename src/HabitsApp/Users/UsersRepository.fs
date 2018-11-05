namespace Users

open Database
open FSharp.Control.Tasks.ContextInsensitive
open Npgsql
open System.Threading.Tasks
open System
open Dapper

module Repository =
  let private _newConn connectionString = new NpgsqlConnection(connectionString)

  let getByOAuthInfo connectionString oauthType oauthId : Task<Result<User option, exn>> =
    task {
      use connection = _newConn connectionString
      let query = @"
SELECT id, oauth_type, oauth_id
FROM Users
WHERE oauth_type = @oauth_type
  AND oauth_id = @oauth_id"

      return! querySingle connection query (Some <| dict ["oauth_type" => oauthType; "oauth_id" => oauthId  ])
    }

  let insert connectionString v: Task<Result<int, exn>> =
    task {
      use connection = _newConn connectionString
      let query = "INSERT INTO Users(oauth_type, oauth_id) VALUES (@oauth_type, @oauth_id)"
      return! execute connection query v
    }
