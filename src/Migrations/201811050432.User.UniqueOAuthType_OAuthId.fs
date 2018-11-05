namespace Migrations

open SimpleMigrations

[<Migration(201811050432L, "Add unique index for oauth_type and oauth_id to Users")>]
type ``Add unique index for oauth_type and oauth_id to Users`` () =
  inherit Migration ()

  override __.Up () = base.Execute @"
-- Too much work for almost no benefit at this point
DELETE FROM Users; -- Cannot TRUNCATE due to FK constraint
ALTER SEQUENCE users_id_seq RESTART WITH 1;

CREATE UNIQUE INDEX users_oauth_type_oauth_id_unique_idx ON Users (oauth_type, oauth_id);
"

  override __.Down () = base.Execute "DROP INDEX users_oauth_type_oauth_id_unique_idx"
