module Index

open Giraffe.GiraffeViewEngine

let index =
  [
    a [_href "/habits"; attr "data-turbolinks" "false"] [rawText "Let's go."]
  ]

let layout =
  App.layout false index
