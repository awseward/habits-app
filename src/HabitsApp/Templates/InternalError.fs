module InternalError

open System
open Giraffe.GiraffeViewEngine

let layout isProduction (ex: Exception)  =
    html [_class "has-navbar-fixed-top"] [
        head [] [
            meta [_charset "utf-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [encodedText "Error 500"]
        ]
        body [] [
           yield h1 [] [rawText "Internal Server Error"]
           if (not isProduction) then
               yield h3 [] [rawText ex.Message]
               yield h4 [] [rawText ex.Source]
               yield p [] [rawText ex.StackTrace]
           yield a [_href "/" ] [rawText "Go back to home page"]
        ]
    ]
