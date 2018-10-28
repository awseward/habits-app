namespace Habits

open System

[<CLIMutable>]
type Habit = {
  id: int
  name: string
}

module Validation =
  let validate v =
    let validators = [
      fun u -> if String.IsNullOrWhiteSpace(u.name) then Some ("name", "Name shouldn't be blank") else None
    ]

    validators
    |> List.fold (fun acc e ->
      match e v with
      | Some (k,v) -> Map.add k v acc
      | None -> acc
    ) Map.empty
