namespace Migrations

open SimpleMigrations

[<Migration(201811050041L, "Create Users")>]
type CreateUsers () =
  inherit Migration()

  override __.Up () = base.Execute @"
CREATE TABLE Users(
  id SERIAL PRIMARY KEY,
  oauth_type TEXT NOT NULL,
  oauth_id TEXT NOT NULL
)"

  override __.Down () = base.Execute "DROP TABLE Users"
