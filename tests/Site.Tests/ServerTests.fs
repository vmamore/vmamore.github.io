module ServerTests

open System.IO
open Xunit
open Site.Server
open TestHelpers

/// <tmp>/site with index.html, en/index.html, style.css; <tmp>/secret.txt sits outside it.
let private site () =
    let parent = tempDir ()
    File.WriteAllText(Path.Combine(parent, "secret.txt"), "x")
    let dir = Path.Combine(parent, "site")
    Directory.CreateDirectory(Path.Combine(dir, "en")) |> ignore
    File.WriteAllText(Path.Combine(dir, "index.html"), "pt")
    File.WriteAllText(Path.Combine(dir, "en", "index.html"), "en")
    File.WriteAllText(Path.Combine(dir, "style.css"), "body{}")
    dir

[<Fact>]
let ``root and folders resolve to index.html`` () =
    let dir = site ()
    Assert.Equal(Some(Path.Combine(dir, "index.html")), resolve dir "/")
    Assert.Equal(Some(Path.Combine(dir, "en", "index.html")), resolve dir "/en/")
    Assert.Equal(Some(Path.Combine(dir, "en", "index.html")), resolve dir "/en")

[<Fact>]
let ``files resolve and missing files do not`` () =
    let dir = site ()
    Assert.Equal(Some(Path.Combine(dir, "style.css")), resolve dir "/style.css")
    Assert.Equal(None, resolve dir "/nope.css")

[<Fact>]
let ``paths outside the site folder are rejected`` () =
    let dir = site ()
    Assert.Equal(None, resolve dir "/../secret.txt")

[<Theory>]
[<InlineData("a.html", "text/html; charset=utf-8")>]
[<InlineData("a.css", "text/css; charset=utf-8")>]
[<InlineData("a.svg", "image/svg+xml")>]
[<InlineData("a.woff2", "font/woff2")>]
[<InlineData("a.png", "image/png")>]
[<InlineData("a.txt", "text/plain; charset=utf-8")>]
[<InlineData("CNAME", "application/octet-stream")>]
let ``content types by extension`` (file: string, expected: string) =
    Assert.Equal(expected, contentType file)
