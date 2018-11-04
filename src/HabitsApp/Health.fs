namespace Habits

open Giraffe.GiraffeViewEngine
open System

module Health =

  let dot (red: byte, green: byte, blue: byte) =
    div [_class "dot-container-container"] [
      div [_class "dot-container"] [
        // FIXME: Find a way to apply this other than using inline styles
        div [_class "dot-outer"; _style (sprintf "background-color: rgba(%d, %d, %d, 0.4)" red green blue)] []
      ]
    ]

  let redToGreenDots =
    let ramp = seq { 0uy .. 255uy }
    let flat = 255uy |> Seq.replicate 255
    flat
    |> Seq.append ramp
    |> fun full -> Seq.zip full (Seq.rev full)
    |> Seq.map (fun (red, green) -> (red, green, 0uy))
    |> Seq.chunkBySize 5
    |> Seq.skip 1
    |> Seq.take 100
    |> Seq.map (Array.head >> dot)
    |> Seq.rev
    |> Seq.toList

  let score (granularity: int) (min: int64) (max: int64) (value: int64) =
    let difference = max - min
    let multiplier = (float (granularity - 1)) / (float difference)

    int <| (float (value - min)) * multiplier

  let scoreDateTimeOffset (min: DateTimeOffset) (max: DateTimeOffset) (value: DateTimeOffset) =
    score 100 (min.ToUnixTimeSeconds()) (max.ToUnixTimeSeconds()) (value.ToUnixTimeSeconds())
