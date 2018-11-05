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

      match value with
      | None -> redDot
      | Some dtOffset ->
          dtOffset
          |> getScore
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
  let private sortHabits (habits: Habit list) =
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

          let sortedHabits = sortHabits habits
          let getDot = HealthDots.forHabit sortedHabits

          for habit in sortedHabits do
            yield div [_class "card-container overflow-auto"] [
              yield span [_class "card-title"] [
                yield! (if HealthDots.enabled then [getDot habit] else [])
                yield (rawText habit.name)
              ]
              yield p [] [rawText <| sprintf "Last done: %s" (_whenOrNever habit.last_done_at)]
              yield span [_class "card-links"] [
                a [_class "button is-text"; _href (Links.withId ctx habit.id )] [rawText "Show"]
                a [_class "button is-text"; _href (Links.edit ctx habit.id )] [rawText "Edit"]
                a [_class "button is-text is-delete"; attr "data-href" (Links.withId ctx habit.id ) ] [rawText "Delete"]
              ]
            ]
        ]
      ]
    ]

  let show (ctx : HttpContext) (o : Habit) =
    let cnt = [
      div [_class "container "] [
        ul [] [
          li [] [ strong [] [rawText "Name: "]; rawText (string o.name) ]
          li [] [ strong [] [rawText "Last Done: "]; rawText (_whenOrNever o.last_done_at) ]
        ]
        a [_class "button is-text"; _href (Links.edit ctx o.id)] [rawText "Edit"]
        a [_class "button is-text"; _href (Links.index ctx )] [rawText "Back"]
      ]
    ]
    App.layout ([section [_class "section"] cnt])

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

  let private createForm (ctx: HttpContext) (habit: HabitToCreate) (validationResult: Map<string, string>) =
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
          yield input [_type "hidden"; _name "user_id"; _value (string habit.user_id)]
          yield field (fun i -> (string i.name)) "Name" "name" false []
          yield field (fun i -> (_whenOrNull i.last_done_at)) "Last Done" "last_done_at" true [
            button [_id "set_last_done_now_button"; _type "button"] [rawText "Set as now"]
          ]
          yield _buttons ctx
        ]
      ]
    ]
    App.layout ([section [_class "section"] cnt])

  let private editForm (ctx: HttpContext) (habit: Habit) (validationResult: Map<string, string>) =
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
    let formActions = Links.withId ctx habit.id
    let cnt = [
      div [_class "container "] [
        form [ _action formActions; _method "post"] [
          if not validationResult.IsEmpty then
            yield _oopsDiv
          yield p [] [rawText habit.name]
          yield input [_type "hidden"; _name "id"; _value (string habit.id)]
          yield input [_type "hidden"; _name "user_id"; _value (string habit.user_id)]
          yield input [_type "hidden"; _name "name"; _value (string habit.name)]
          yield field (fun i -> (_whenOrNull i.last_done_at)) "Last Done" "last_done_at" true [
            button [_id "set_last_done_now_button"; _type "button"] [rawText "Set as now"]
          ]
          yield _buttons ctx
        ]
      ]
    ]
    App.layout ([section [_class "section"] cnt])

  let add (ctx: HttpContext) (habit: HabitToCreate) (validationResult: Map<string, string>) =
    createForm ctx habit validationResult

  let edit (ctx: HttpContext) (habit: Habit) (validationResult : Map<string, string>) =
    editForm ctx habit validationResult
