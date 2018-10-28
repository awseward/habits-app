namespace Habits

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.ContextInsensitive
open Config
open Saturn

module Controller =

  let indexAction (ctx : HttpContext) =
    task {
      let cnf = Controller.getConfig ctx
      let! result = Database.getAll cnf.connectionString
      match result with
      | Ok result ->
        return Views.index ctx (List.ofSeq result)
      | Error ex ->
        return raise ex
    }

  let showAction (ctx: HttpContext) (id : int) =
    task {
      let cnf = Controller.getConfig ctx
      let! result = Database.getById cnf.connectionString id
      match result with
      | Ok (Some result) ->
        return Views.show ctx result
      | Ok None ->
        return NotFound.layout
      | Error ex ->
        return raise ex
    }

  let addAction (ctx: HttpContext) =
    task {
      return Views.add ctx { HabitToCreate.name = "" } Map.empty
    }

  let editAction (ctx: HttpContext) (id : int) =
    task {
      let cnf = Controller.getConfig ctx
      let! result = Database.getById cnf.connectionString id
      match result with
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
        let! result = Database.insert cnf.connectionString habitToCreate
        match result with
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
        let! result = Database.update cnf.connectionString habit
        match result with
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
      let! result = Database.delete cnf.connectionString id
      match result with
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
