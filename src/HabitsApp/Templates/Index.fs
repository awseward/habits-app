module Index

open Giraffe.GiraffeViewEngine

let index =
  [
    a [_href "/habits"] [rawText "Do it."]
  ]

let layout =
  App.layout index
