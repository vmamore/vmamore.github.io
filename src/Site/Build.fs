module Site.Build

open System
open System.IO
open Markdig
open Site.Domain
open Site.Views // brings PageModel's record labels into scope

let findRoot (start: string) =
    let rec go (dir: DirectoryInfo) =
        if isNull dir then
            failwithf "Could not find the repo root (a folder with content/ and static/) above %s" start
        elif Directory.Exists(Path.Combine(dir.FullName, "content"))
             && Directory.Exists(Path.Combine(dir.FullName, "static")) then
            dir.FullName
        else
            go dir.Parent
    go (DirectoryInfo start)

let private withSeparator (path: string) =
    Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
    + string Path.DirectorySeparatorChar

/// The build wipes outDir first, so refuse anything that would delete source:
/// inside the repo only `_site` is allowed; outside it, anything but an ancestor of the repo.
let isSafeOutDir (root: string) (outDir: string) =
    let repo = withSeparator root
    let out = withSeparator outDir
    let site = withSeparator (Path.Combine(root, "_site"))
    let ignoreCase = StringComparison.OrdinalIgnoreCase
    if out.StartsWith(repo, ignoreCase) then out.StartsWith(site, ignoreCase)
    else not (repo.StartsWith(out, ignoreCase))

let renderAbout (markdown: string) (years: int) =
    Markdown.ToHtml(markdown.Replace("{{years}}", string years))

let rec private copyDir (source: string) (target: string) =
    Directory.CreateDirectory target |> ignore
    for file in Directory.GetFiles source do
        File.Copy(file, Path.Combine(target, Path.GetFileName file), true)
    for dir in Directory.GetDirectories source do
        copyDir dir (Path.Combine(target, Path.GetFileName dir))

let run (root: string) (outDir: string) (today: YearMonth) =
    if not (isSafeOutDir root outDir) then
        invalidOp (sprintf "Refusing to build into %s: it would delete source files." outDir)
    if Directory.Exists outDir then
        Directory.Delete(outDir, true)
    copyDir (Path.Combine(root, "static")) outDir
    let years = I18n.fullYearsBetween (Profile.careerStart Data.profile) today
    for lang in [ Pt; En ] do
        let markdown = File.ReadAllText(Path.Combine(root, "content", sprintf "about.%s.md" (I18n.code lang)))
        let html =
            Views.render
                { Lang = lang
                  Profile = Data.profile
                  AboutHtml = renderAbout markdown years
                  Today = today }
        let file = Path.Combine(outDir, I18n.outputFile lang)
        Directory.CreateDirectory(Path.GetDirectoryName file) |> ignore
        File.WriteAllText(file, html)
