namespace Migrations
open SimpleMigrations

[<Migration(201810282154L, "Add last_done_at to Habits")>]
type ``Add last_done_at to Habits`` () =
  inherit Migration ()

  override __.Up () =
    base.Execute "ALTER TABLE Habits ADD COLUMN last_done_at TIMESTAMP WITH TIME ZONE"

  override __.Down () =
    base.Execute "ALTER TABLS Habits DROP COLUMN last_done_at"
