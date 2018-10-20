namespace Migrations
open SimpleMigrations

[<Migration(201810200231L, "Create Habits")>]
type CreateHabits() =
  inherit Migration()

  override __.Up() =
    base.Execute(@"CREATE TABLE Habits(
      id SERIAL NOT NULL PRIMARY KEY,
      name TEXT NOT NULL
    )")

  override __.Down() =
    base.Execute(@"DROP TABLE Habits")
