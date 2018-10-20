module App

open Giraffe.GiraffeViewEngine

let layout (content: XmlNode list) =
    html [_class "has-navbar-fixed-top"] [
        head [] [
            meta [_charset "utf-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [encodedText "Hello SaturnSample"]
            link [_rel "stylesheet"; _href "/app.css" ]
        ]

        body [] [
            yield! content
            yield script [_src "/app.js"] []
        ]
    ]
