namespace Habits

[<CLIMutable>]
type Habit = {
  id: System.Guid
  name: string
}

module Validation =
  let validate v =
    Map.empty
    // let validators = [
    //   fun u -> if isNull u.id then Some ("id", "Id shouldn't be empty") else None
    // ]

    // validators
    // |> List.fold (fun acc e ->
    //   match e v with
    //   | Some (k,v) -> Map.add k v acc
    //   | None -> acc
    // ) Map.empty
