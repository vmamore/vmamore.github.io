module DataTests

open System.Text.RegularExpressions
open Xunit
open Site.Domain
open Site.Data
open TestHelpers

[<Fact>]
let ``career starts in August 2018`` () =
    Assert.Equal(ym 2018 8, Profile.careerStart profile)

[<Fact>]
let ``links are https only and there is no email`` () =
    for (_, url) in profile.Links do
        Assert.StartsWith("https://", url)
        notContains "mailto:" url
        notContains "@" url
    Assert.Equal<string list>([ "GitHub"; "LinkedIn" ], profile.Links |> List.map fst)

[<Fact>]
let ``companies and roles are newest first`` () =
    let starts = profile.Companies |> List.map (Company.span >> fst)
    Assert.Equal<YearMonth list>(List.sortDescending starts, starts)
    for c in profile.Companies do
        let roleStarts = c.Roles |> List.map (fun r -> r.Start)
        Assert.Equal<YearMonth list>(List.sortDescending roleStarts, roleStarts)

[<Fact>]
let ``only Mindbody is current, as SWE III on the Branded App`` () =
    let current =
        [ for c in profile.Companies do
              for r in c.Roles do
                  if r.End.IsNone then c.Name, r ]
    let name, role = Assert.Single current
    Assert.Equal("Mindbody", name)
    Assert.Equal({ Pt = "SWE III"; En = "SWE III" }, role.Title)
    contains "Branded App" (Localized.get Pt role.Summary.Value)
    contains "Branded App" (Localized.get En role.Summary.Value)
    Assert.Contains(".NET 10", role.Tech)
    Assert.Contains("Fastlane", role.Tech)

[<Fact>]
let ``headline is SWE III at Mindbody in both languages`` () =
    Assert.Equal({ Pt = "SWE III @ Mindbody"; En = "SWE III @ Mindbody" }, profile.Headline)

[<Fact>]
let ``company dirs are unique lowercase folder names`` () =
    let dirs = profile.Companies |> List.map (fun c -> c.Dir)
    Assert.Equal(List.length dirs, List.length (List.distinct dirs))
    for d in dirs do
        Assert.Matches(Regex("^[a-z0-9-]+$"), d)

[<Fact>]
let ``Stone has three roles spanning Dec 2020 to Dec 2025`` () =
    let stone = profile.Companies |> List.find (fun c -> c.Name = "Stone")
    Assert.Equal(3, List.length stone.Roles)
    Assert.Equal((ym 2020 12, Some(ym 2025 12)), Company.span stone)
