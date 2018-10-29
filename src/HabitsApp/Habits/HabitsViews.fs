namespace Habits

open Microsoft.AspNetCore.Http
open Giraffe.GiraffeViewEngine
open Saturn
open System

module Views =
  let private _whenOr (defaultValue: string) (value: DateTimeOffset option) =
    match value with
    | Some dto -> (string dto)
    | None -> defaultValue
  let private _whenOrNever = _whenOr "Never"
  let private _whenOrNull = _whenOr null

  let index (ctx : HttpContext) (objs : Habit list) =
    let cnt = [
      div [_class "container "] [
        h2 [ _class "title"] [rawText "Listing Habits"]

        table [_class "table is-hoverable is-fullwidth"] [
          thead [] [
            tr [] [
              th [] [rawText "Id"]
              th [] [rawText "Name"]
              th [] [rawText "Last Done"]
              th [] []
            ]
          ]
          tbody [] [
            for o in objs do
              yield tr [] [
                td [] [rawText (string o.id)]
                td [] [rawText (string o.name)]
                td [] [rawText (_whenOrNever o.last_done_at)]
                td [] [
                  a [_class "button is-text"; _href (Links.withId ctx o.id )] [rawText "Show"]
                  a [_class "button is-text"; _href (Links.edit ctx o.id )] [rawText "Edit"]
                  a [_class "button is-text is-delete"; attr "data-href" (Links.withId ctx o.id ) ] [rawText "Delete"]
                ]
              ]
          ]
        ]

        a [_class "button is-text"; _href (Links.add ctx )] [rawText "New Habit"]
      ]
    ]
    App.layout ([section [_class "section"] cnt])

  let show (ctx : HttpContext) (o : Habit) =
    let cnt = [
      div [_class "container "] [
        h2 [ _class "title"] [rawText "Show Habit"]

        ul [] [
          li [] [ strong [] [rawText "Id: "]; rawText (string o.id) ]
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
    let field selector lbl key =
      div [_class "field"] [
        yield label [_class "label"] [rawText (string lbl)]
        yield div [_class "control has-icons-right"] [
          yield input [_class (if validationResult.ContainsKey key then "input is-danger" else "input"); _value (selector habit); _name key ; _type "text" ]
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
          yield field (fun i -> (string i.name)) "Name" "name"
          yield field (fun i -> (_whenOrNull i.last_done_at)) "Last Done" "last_done_at"
          yield _buttons ctx
        ]
      ]
    ]
    App.layout ([section [_class "section"] cnt])

  let private editForm (ctx: HttpContext) (habit: Habit) (validationResult: Map<string, string>) =
    let field selector lbl key =
      div [_class "field"] [
        yield label [_class "label"] [rawText (string lbl)]
        yield div [_class "control has-icons-right"] [
          yield input [_class (if validationResult.ContainsKey key then "input is-danger" else "input"); _value (selector habit); _name key ; _type "text" ]
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
          yield (p [] [(rawText (string habit.id))])
          yield input [_type "hidden"; _name "id"; _value (string habit.id)]
          yield field (fun i -> (string i.name)) "Name" "name"
          yield field (fun i -> (_whenOrNull i.last_done_at)) "Last Done" "last_done_at"
          yield _buttons ctx
        ]
      ]
    ]
    App.layout ([section [_class "section"] cnt])

  let add (ctx: HttpContext) (habit: HabitToCreate) (validationResult: Map<string, string>) =
    createForm ctx habit validationResult

  let edit (ctx: HttpContext) (habit: Habit) (validationResult : Map<string, string>) =
    editForm ctx habit validationResult
