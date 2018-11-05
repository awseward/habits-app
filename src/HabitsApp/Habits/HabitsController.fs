namespace Habits

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.ContextInsensitive
open Config
open Saturn

module Controller =
  let private _getUserId (ctx: HttpContext) =
    ctx.Items.["user_id"] :?> int

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

  let showAction (ctx: HttpContext) (id : int) =
    task {
      let cnf = Controller.getConfig ctx
      let userId = _getUserId ctx

      match! Database.getByUserIdAndId cnf.connectionString userId id with
      | Ok (Some result) ->
          return Views.show ctx result
      | Ok None ->
          return NotFound.layout
      | Error ex ->
          return raise ex
    }

  let addAction (ctx: HttpContext) =
    task {
      let userId = _getUserId ctx

      return Views.add ctx (HabitToCreate.GetEmpty userId) Map.empty
    }

  let editAction (ctx: HttpContext) (id : int) =
    task {
      let cnf = Controller.getConfig ctx
      let userId = _getUserId ctx
      match! Database.getByUserIdAndId cnf.connectionString userId id with
      | Ok (Some result) ->
          return Views.edit ctx result Map.empty
      | Ok None ->
          return NotFound.layout
      | Error ex ->
          return raise ex
    }

  let createAction (ctx: HttpContext) =
    task {
      let! habitToCreate = Controller.getModel<HabitToCreate> ctx
      let validateResult = Validation.validateCreate habitToCreate
      if validateResult.IsEmpty then
        let cnf = Controller.getConfig ctx
        match! Database.insert cnf.connectionString habitToCreate with
        | Ok _ ->
            return! Controller.redirect ctx (Links.index ctx)
        | Error ex ->
            return raise ex
      else
        return! Controller.renderHtml ctx (Views.add ctx habitToCreate validateResult)
    }

  let updateAction (ctx: HttpContext) (id : int) =
    task {
      let! habit = Controller.getModel<Habit> ctx
      let validateResult = Validation.validateUpdate habit
      if validateResult.IsEmpty then
        let cnf = Controller.getConfig ctx
        match! Database.update cnf.connectionString habit with
        | Ok _ ->
            return! Controller.redirect ctx (Links.index ctx)
        | Error ex ->
            return raise ex
      else
        return! Controller.renderHtml ctx (Views.edit ctx habit validateResult)
    }

  let deleteAction (ctx: HttpContext) (id : int) =
    task {
      let cnf = Controller.getConfig ctx
      match! Database.delete cnf.connectionString id with
      | Ok _ ->
          return! Controller.redirect ctx (Links.index ctx)
      | Error ex ->
          return raise ex
    }

  let resource = controller {
    index indexAction
    show showAction
    add addAction
    edit editAction
    create createAction
    update updateAction
    delete deleteAction
  }
