namespace Habits

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.ContextInsensitive
open Config
open Saturn
open System

module Controller =
  let private _getUserId = Users.Service.getCurrentUserId

  let indexAction (ctx : HttpContext) =
    task {
      let cnf = Controller.getConfig ctx
      let userId = _getUserId ctx

      match! Database.getAllForUserId cnf.connectionString userId with
      | Ok result ->
          return Views.index ctx (List.ofSeq result)
      | Error ex ->
          return raise ex
    }

  let addAction (ctx: HttpContext) =
    task {
      let userId = _getUserId ctx

      return Views.add ctx (HabitToCreate.GetEmpty userId) Map.empty
    }

  let createAction (ctx: HttpContext) =
    let getModel () = task {
      let! boundModel = Controller.getModel<HabitToCreate> ctx
      let userId = _getUserId ctx
      return { boundModel with user_id = userId }
    }

    task {
      let! model = getModel ()
      let validateResult = Validation.validateCreate model
      if validateResult.IsEmpty then
        let cnf = Controller.getConfig ctx
        match! Database.insert cnf.connectionString model with
        | Ok _ ->
            return! Controller.redirect ctx (Links.index ctx)
        | Error ex ->
            return raise ex
      else
        return! Controller.renderHtml ctx (Views.add ctx model validateResult)
    }

  let updateAction (ctx: HttpContext) (id : int) =
    let getModel () = task {
      let! boundModel = Controller.getModel<Habit> ctx
      let userId = _getUserId ctx

      return
        { boundModel with
            id = id
            last_done_at = Some DateTimeOffset.Now
            user_id = userId }
    }

    task {
      let cnf = Controller.getConfig ctx
      let! model = getModel ()
      let validateResult = Validation.validateUpdate model
      if validateResult.IsEmpty then
        match! Database.update cnf.connectionString model with
        | Ok _ ->
            return! Controller.redirect ctx (Links.index ctx)
        | Error ex ->
            return raise ex
      else
        // TODO: Maybe render with some kind of indication of validation errors...
        return! Controller.redirect ctx (Links.index ctx)
    }

  let deleteAction (ctx: HttpContext) (id : int) =
    task {
      let cnf = Controller.getConfig ctx
      let userId = _getUserId ctx
      match! Database.delete cnf.connectionString userId id with
      | Ok _ ->
          return! Controller.redirect ctx (Links.index ctx)
      | Error ex ->
          return raise ex
    }

  let resource = controller {
    index indexAction
    add addAction
    create createAction
    update updateAction
    delete deleteAction
  }
