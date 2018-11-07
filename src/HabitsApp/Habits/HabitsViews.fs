namespace Habits

open Microsoft.AspNetCore.Http
open Giraffe.GiraffeViewEngine
open Saturn
open System

module Views =

  module HealthDots =
    let forDateTimeOffset (min: DateTimeOffset) (max: DateTimeOffset) (value: DateTimeOffset option) =
      let getScore = Health.scoreDateTimeOffset min max
      let redDot = Health.redToGreenDots |> List.head
      let guardRailsBandaid score =
        score
        |> fun s -> Math.Max (s, 0)
        |> fun s -> Math.Min (s, Health.redToGreenDots.Length)
        |> fun s ->
            if s <> score then
              eprintfn "WARNING: guard rails required for sloppy score calculation to prevent IndexOutOfBoundsException"
            s

      match value with
      | None -> redDot
      | Some dtOffset ->
          dtOffset
          |> getScore
          |> guardRailsBandaid
          |> fun score -> List.item score Health.redToGreenDots

    let forHabit (sortedHabits: Habit list):  (Habit -> XmlNode) =
      let freshest = DateTimeOffset.Now
      let stalest =
        sortedHabits
        |> List.map (fun h -> h.last_done_at)
        |> List.filter Option.isSome
        |> function
            | (Some dtOffset)::_ -> dtOffset
            | _ -> freshest

      (fun h -> forDateTimeOffset stalest freshest h.last_done_at)

    let enabled = true // FIXME: Make this user-configurable

  let private _whenOr (defaultValue: string) (value: DateTimeOffset option) =
    match value with
    | Some dto -> (string dto)
    | None -> defaultValue
  let private _whenOrNever = _whenOr "Never"
  let private _whenOrNull = _whenOr null
  let private _sortHabits (habits: Habit list) =
    habits
    |> List.sortBy (fun h ->
        match h.last_done_at with
        | Some dto -> dto
        | None -> DateTimeOffset.MinValue
    )

  let index (ctx : HttpContext) (habits : Habit list) =
    App.layout [
      section [_class "section"] [
        yield div [_class "container "] [
          yield div [_class "overflow-auto"] [
            a [_class "button is-text new-habit-button"; _href (Links.add ctx )] [rawText "New Habit"]
          ]

          let sortedHabits = _sortHabits habits
          let getDot = HealthDots.forHabit sortedHabits

          for habit in sortedHabits do
            yield div [_class "card-container overflow-auto"] [
              yield span [_class "card-title"] [
                yield! (if HealthDots.enabled then [getDot habit] else [])
                yield (rawText habit.name)
                yield a [_class "button is-delete"; attr "data-href" (Links.withId ctx habit.id); _href ""] [rawText "Delete"]
              ]
              yield p [] [rawText <| sprintf "Last done: %s" (_whenOrNever habit.last_done_at)]
              yield span [_class "card-links"] [
                form [_action (Links.withId ctx habit.id); _method "post"] [
                  input [_type "hidden"; _name "name"; _value habit.name]
                  button [_type "submit"; _class "button is-link"] [rawText "Now"]
                ]
              ]
            ]
        ]
      ]
    ]

  let private _oopsDiv =
    div [_class "notification is-danger"] [
      a [_class "delete"; attr "aria-label" "delete"] []
      rawText "Oops, something went wrong! Please check the errors below."
    ]

  let private _buttons (ctx: HttpContext) =
    div [_class "field is-grouped"] [
      div [_class "control"] [
        input [_type "submit"; _class "button is-link"; _value "Submit"]
      ]
      div [_class "control"] [
        a [_class "button is-text"; _href (Links.index ctx)] [rawText "Cancel"]
      ]
    ]

  let private _createForm (ctx: HttpContext) (habit: HabitToCreate) (validationResult: Map<string, string>) =
    let field selector lbl key inputReadOnly (moreStuff: XmlNode list) =
      div [_class "field"] [
        yield label [_class "label"] [rawText (string lbl)]
        yield div [_class "control has-icons-right"] [
          yield input [_class (if validationResult.ContainsKey key then "input is-danger" else "input"); _value (selector habit); _name key ; _type "text"; (if inputReadOnly then _readonly else attr "" "")]
          yield span [] moreStuff
          if validationResult.ContainsKey key then
            yield span [_class "icon is-small is-right"] [
              i [_class "fas fa-exclamation-triangle"] []
            ]
        ]
        if validationResult.ContainsKey key then
          yield p [_class "help is-danger"] [rawText validationResult.[key]]
      ]
    let formActions = Links.index ctx
    let cnt = [
      div [_class "container "] [
        form [ _action formActions; _method "post"] [
          if not validationResult.IsEmpty then
            yield _oopsDiv
          yield field (fun i -> (string i.name)) "Name" "name" false []
          yield field (fun i -> (_whenOrNull i.last_done_at)) "Last Done" "last_done_at" true [
            button [_id "set_last_done_now_button"; _type "button"] [rawText "Now"]
          ]
          yield _buttons ctx
        ]
      ]
    ]
    App.layout ([section [_class "section"] cnt])

  let add (ctx: HttpContext) (habit: HabitToCreate) (validationResult: Map<string, string>) =
    _createForm ctx habit validationResult
