namespace Habits

open System

[<CLIMutable>]
type Habit = {
  id: int
  name: string
}
[<CLIMutable>]
type HabitToCreate = {
  name: string
}

type CreateOrUpdate = Choice<HabitToCreate, Habit>

module Validation =
  let validateCreate (habit: HabitToCreate) =
    let validators = [
      fun u -> if String.IsNullOrWhiteSpace(u.name) then Some ("name", "Name shouldn't be blank") else None
    ]

    validators
    |> List.fold (fun acc fn ->
      match fn habit with
      | Some (k,v) -> Map.add k v acc
      | None -> acc
    ) Map.empty

  let validateUpdate (habit: Habit) =
    let validators = [
      fun (u: Habit) -> if String.IsNullOrWhiteSpace(u.name) then Some ("name", "Name shouldn't be blank") else None
    ]

    validators
    |> List.fold (fun acc fn ->
      match fn habit with
      | Some (k,v) -> Map.add k v acc
      | None -> acc
    ) Map.empty
