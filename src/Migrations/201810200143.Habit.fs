namespace Migrations
open SimpleMigrations

[<Migration(201810200143L, "Create Habits")>]
type CreateHabits() =
  inherit Migration()

  override __.Up() =
    base.Execute(@"CREATE TABLE Habits(
      id TEXT NOT NULL,
      name TEXT NOT NULL
    )")

  override __.Down() =
    base.Execute(@"DROP TABLE Habits")
