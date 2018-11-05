namespace Migrations

open SimpleMigrations

//201811050336.Habit.UserId.fs
[<Migration(201811050336L, "Add user_id to Habits")>]
type ``Add user_id to Habits`` () =
  inherit Migration ()

  override __.Up () = base.Execute @"
TRUNCATE TABLE Habits; -- Too much work for almost no benefit at this point

ALTER TABLE Habits
ADD COLUMN user_id integer NOT NULL;

ALTER TABLE Habits
ADD CONSTRAINT FK_habit_user
FOREIGN KEY (user_id)
REFERENCES Users (id)
ON DELETE CASCADE;"

  override __.Down () = base.Execute @"
ALTER TABLE Habits DROP CONSTRAINT FK_habit_user;

ALTER TABLE Habits DROP COLUMN user_id;"
