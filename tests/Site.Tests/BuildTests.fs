module BuildTests

open System
open System.IO
open System.Text.RegularExpressions
open Xunit
open Site
open TestHelpers

let private root = Build.findRoot AppContext.BaseDirectory
let private today = ym 2026 11

let private build () =
    let out = Path.Combine(tempDir (), "_site")
    Build.run root out today
    out

[<Fact>]
let ``findRoot finds the folder with content and static`` () =
    Assert.True(Directory.Exists(Path.Combine(root, "content")))
    Assert.True(File.Exists(Path.Combine(root, "static", "CNAME")))

[<Fact>]
let ``renderAbout replaces the years token and renders markdown`` () =
    Assert.Equal("<p>com 8 anos <em>x</em></p>\n", Build.renderAbout "com {{years}} anos *x*" 8)

[<Fact>]
let ``isSafeOutDir refuses the repo, its ancestors and its source folders`` () =
    let repo = Path.Combine(Path.GetTempPath(), "r", "repo")
    Assert.False(Build.isSafeOutDir repo repo)
    Assert.False(Build.isSafeOutDir repo (Path.Combine(Path.GetTempPath(), "r")))
    Assert.False(Build.isSafeOutDir repo (Path.Combine(repo, "content")))
    Assert.False(Build.isSafeOutDir repo (Path.Combine(repo, "src")))
    Assert.True(Build.isSafeOutDir repo (Path.Combine(repo, "_site")))
    Assert.True(Build.isSafeOutDir repo (Path.Combine(Path.GetTempPath(), "elsewhere", "_site")))

[<Fact>]
let ``build writes both pages and copies static files recursively`` () =
    let out = build ()
    for file in
        [ "index.html"; "pt/index.html"; "style.css"; "CNAME"; "favicon.svg"
          "fonts/JetBrainsMono-Regular.woff2"; "fonts/JetBrainsMono-SemiBold.woff2"; "fonts/OFL.txt" ] do
        Assert.True(File.Exists(Path.Combine(out, file)), sprintf "missing %s" file)
    Assert.Equal("vmamore.com.br", File.ReadAllText(Path.Combine(out, "CNAME")).Trim())

[<Fact>]
let ``build fills in years of experience in both languages`` () =
    let out = build ()
    let en = File.ReadAllText(Path.Combine(out, "index.html"))
    let pt = File.ReadAllText(Path.Combine(out, "pt", "index.html"))
    contains "8 anos de experiência" pt
    contains "8 years of experience" en
    notContains "{{years}}" pt
    notContains "{{years}}" en
    contains "<html lang=\"pt-BR\">" pt
    contains "<html lang=\"en\">" en

[<Fact>]
let ``every root-relative link and font url resolves to a built file`` () =
    let out = build ()
    let pages = [ "index.html"; "pt/index.html" ] |> List.map (fun f -> File.ReadAllText(Path.Combine(out, f)))
    let css = File.ReadAllText(Path.Combine(out, "style.css"))
    let fromPages =
        pages
        |> List.collect (fun html ->
            [ for m in Regex.Matches(html, "(?:href|src)=\"(/[^\"]*)\"") -> m.Groups.[1].Value ])
    let fromCss = [ for m in Regex.Matches(css, "url\\(\"(/[^\"]+)\"\\)") -> m.Groups.[1].Value ]
    let urls = fromPages @ fromCss |> List.distinct
    Assert.Contains("/pt/", urls)
    Assert.Contains("/fonts/JetBrainsMono-SemiBold.woff2", urls)
    for url in urls do
        Assert.True((Server.resolve out url).IsSome, sprintf "broken link %s" url)

[<Fact>]
let ``every company in the profile has a logo file in static`` () =
    for company in Data.profile.Companies do
        let logo = Assert.IsType<string>(company.Logo |> Option.toObj)
        Assert.StartsWith("/logos/", logo)
        Assert.True(File.Exists(Path.Combine(root, "static", logo.TrimStart('/'))), sprintf "missing %s" logo)

[<Fact>]
let ``a rebuild wipes stale files`` () =
    let out = build ()
    File.WriteAllText(Path.Combine(out, "stale.txt"), "old")
    Build.run root out today
    Assert.False(File.Exists(Path.Combine(out, "stale.txt")))
