namespace Habits

open System

[<CLIMutable>]
type Habit = {
  id: int
  name: string
  last_done_at: DateTimeOffset option
}

[<CLIMutable>]
type HabitToCreate = {
  name: string
  last_done_at: DateTimeOffset option
}
  with static member GetEmpty () = { name = ""; last_done_at = None}

type CreateOrUpdate = Choice<HabitToCreate, Habit>

module LastDoneScore =

  let getScoringTransform (min: DateTimeOffset) (max: DateTimeOffset) (value: DateTimeOffset) =
    let minSeconds = min.ToUnixTimeSeconds ()
    let maxSeconds = max.ToUnixTimeSeconds ()
    let elapsedSeconds = maxSeconds - minSeconds
    let multiplier = 99. / (float elapsedSeconds)

    value.ToUnixTimeSeconds ()
    |> fun sec -> (sec - minSeconds)
    |> float
    |> fun secF -> secF * multiplier
    |> int

module Validation =
  let private _nameNotBlank (name: string) =
    if String.IsNullOrWhiteSpace name then
      Some ("name", "Name shouldn't be blank")
    else
      None

  let private _lastDoneAtInThePast (lastDoneAt: DateTimeOffset) =
    if lastDoneAt > DateTimeOffset.Now then
      Some ("last_done_at", "Can't have done this in the future")
    else
      None

  let validateCreate (habit: HabitToCreate) =
    let validators = [
      fun u -> u.name |> _nameNotBlank
      fun u -> u.last_done_at |> Option.bind _lastDoneAtInThePast
    ]

    validators
    |> List.fold (fun acc fn ->
      match fn habit with
      | Some (k,v) -> Map.add k v acc
      | None -> acc
    ) Map.empty

  let validateUpdate (habit: Habit) =
    let validators = [
      fun (u: Habit) -> u.name |> _nameNotBlank
      fun u -> u.last_done_at |> Option.bind _lastDoneAtInThePast
    ]

    validators
    |> List.fold (fun acc fn ->
      match fn habit with
      | Some (k,v) -> Map.add k v acc
      | None -> acc
    ) Map.empty
