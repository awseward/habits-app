namespace Habits

[<CLIMutable>]
type Habit = {
  id: int
  name: string
}

module Validation =
  let validate v =
    Map.empty
    // let validators = [
    //   fun u -> if u.id = 0 then Some ("id", "Id shouldn't be 0") else None
    // ]

    // validators
    // |> List.fold (fun acc e ->
    //   match e v with
    //   | Some (k,v) -> Map.add k v acc
    //   | None -> acc
    // ) Map.empty
