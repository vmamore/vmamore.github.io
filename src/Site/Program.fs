module Site.Program

open System
open System.IO
open Site.Domain

let private build root outDir today =
    Build.run root outDir today
    printfn "Built %s" outDir

[<EntryPoint>]
let main argv =
    let root = Build.findRoot Environment.CurrentDirectory
    let today = YearMonth.ofDateTime DateTime.Today
    let defaultOut = Path.Combine(root, "_site")
    match List.ofArray argv with
    | [ "build" ] ->
        build root defaultOut today
        0
    | [ "build"; out ] ->
        build root (Path.GetFullPath out) today
        0
    | [ "serve" ] ->
        build root defaultOut today
        Server.serve defaultOut 5000
        0
    | _ ->
        eprintfn "usage: dotnet run --project src/Site -- build [outDir] | serve"
        1
