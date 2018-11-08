module App

open Giraffe.GiraffeViewEngine

let layout isAuthenticated (content: XmlNode list) =
    html [_class "has-navbar-fixed-top"] [
        head [] [
            meta [_charset "utf-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [encodedText "Habits"]
            link [_rel "stylesheet"; _href "/app.css" ]
            script [_src "https://cdnjs.cloudflare.com/ajax/libs/turbolinks/5.1.1/turbolinks.js"] []
            script [_defer; _src "/app.js"] []
        ]

        body [] [
            if isAuthenticated then
              yield a [_href "/logout"; _class "logout-link"; attr "data-turbolinks" "false"] [rawText "Log out"]
            yield h1 [] [rawText "Habits"]
            yield! content
        ]
    ]
