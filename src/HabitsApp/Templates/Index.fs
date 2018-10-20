module Index

open Giraffe.GiraffeViewEngine

let index =
    [
        h1 [] [rawText "Hello from Saturn Framework"]
        p  [] [rawText "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec ultricies elit id rhoncus rutrum. Donec ut risus euismod nulla molestie eleifend. Donec eget congue sem, nec feugiat tellus. Nullam dignissim, eros non ultricies dapibus, felis tortor tincidunt orci, vel pretium est nulla sollicitudin eros. Duis tempor ligula eget dolor posuere, nec consectetur eros varius. Fusce posuere dui at lacus rutrum lacinia. Pellentesque convallis dolor sem, non vestibulum dolor accumsan et. Ut consequat felis ut consequat pellentesque. Integer ut fringilla turpis."]
        p  [] [rawText "Ut molestie dolor lacus, ac blandit tortor aliquet eget. Nam sit amet tempor odio. Aenean magna neque, mollis a nunc at, rutrum posuere risus. Sed sit amet malesuada sem. Etiam vitae velit malesuada, rutrum ipsum a, iaculis ligula. Nullam ut ultrices mi. Curabitur turpis neque, pharetra non purus vitae, tincidunt elementum neque. Vivamus efficitur elit in felis volutpat, euismod convallis velit fringilla. Duis tempus, nisl in ornare tempor, est dolor cursus ligula, in aliquet risus ipsum nec erat. Nam vel turpis vestibulum, vestibulum massa eu, maximus justo. Pellentesque vitae ullamcorper leo. Cras quis mattis nibh. Duis ultricies mauris leo, non lacinia risus sollicitudin id. Nam sit amet dolor risus. Curabitur maximus semper velit eleifend ullamcorper. Donec dignissim nibh eu purus ullamcorper, eu vulputate arcu posuere."]
        p  [] [rawText "Quisque dictum pharetra est sit amet varius. Praesent sit amet consectetur quam. Vestibulum posuere turpis lectus, suscipit consequat felis cursus eget. Fusce porta lorem ut lacus malesuada, vel volutpat nulla interdum. Curabitur vulputate molestie leo. Cras ac facilisis nunc. Ut ultrices bibendum mi, in sodales ante congue vel. Aliquam at molestie felis. Vivamus eleifend nisl est, sed blandit sapien sollicitudin quis."]
        p  [] [rawText "Cras tincidunt nibh lorem, at volutpat turpis malesuada at. Etiam rhoncus, orci et auctor pellentesque, purus elit facilisis metus, vel molestie elit risus a tellus. Aenean lectus justo, ullamcorper vel purus vitae, pretium mollis ante. Maecenas sagittis nulla et est cursus ultricies. Quisque porta nibh in blandit malesuada. Vestibulum blandit porta lacus at sollicitudin. Morbi tincidunt nec urna id dapibus. Integer massa elit, dapibus quis malesuada sodales, posuere et ligula. Nunc non nunc in nisl malesuada rhoncus ut in diam. Cras in lacus dui. Nunc diam sapien, tristique sed sapien sit amet, auctor eleifend lacus. Pellentesque sed metus sed justo pharetra dictum in quis velit. Aliquam erat volutpat. Fusce fringilla pharetra blandit."]
        p  [] [rawText "In in purus nisi. Aliquam rutrum bibendum malesuada. Cras faucibus congue est nec fringilla. Mauris interdum ante eu ultricies tincidunt. Donec in metus neque. Pellentesque malesuada mattis tellus fermentum lacinia. Vestibulum ut justo tortor. Integer pellentesque gravida diam sed imperdiet."]
        hr []
        h3 [] [rawText "Something Else"]
        ul [] [
            li [] [rawText "Example of some bullet points"]
            li [] [rawText "Another"]
            li [] [rawText "Not sure exactly what else to put here for now"]
        ]

    ]

let layout =
    App.layout index
