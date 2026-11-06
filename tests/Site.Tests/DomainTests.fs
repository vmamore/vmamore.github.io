module DomainTests

open System
open Xunit
open Site.Domain
open TestHelpers

let role start finish : Role =
    { Title = { Pt = "Dev"; En = "Dev" }
      Start = start
      End = finish
      Summary = None
      Tech = []
      Remote = true }

[<Fact>]
let ``Localized.get picks the requested language`` () =
    let greeting = { Pt = "olá"; En = "hello" }
    Assert.Equal("olá", Localized.get Pt greeting)
    Assert.Equal("hello", Localized.get En greeting)

[<Fact>]
let ``YearMonth.ofDateTime keeps year and month`` () =
    Assert.Equal(ym 2026 11, YearMonth.ofDateTime (DateTime(2026, 11, 5)))

[<Fact>]
let ``Company.span goes from the earliest start to the latest end`` () =
    let stone: Company =
        { Name = "Stone"
          Dir = "stone"; Logo = None; Url = "https://x.example"; Location = "X"
          Roles =
            [ role (ym 2025 4) (Some(ym 2025 12))
              role (ym 2021 12) (Some(ym 2025 3))
              role (ym 2020 12) (Some(ym 2021 11)) ] }
    Assert.Equal((ym 2020 12, Some(ym 2025 12)), Company.span stone)

[<Fact>]
let ``Company.span is open-ended when any role is current`` () =
    let acme: Company =
        { Name = "Acme"
          Dir = "acme"; Logo = None; Url = "https://x.example"; Location = "X"
          Roles = [ role (ym 2024 1) None; role (ym 2022 1) (Some(ym 2023 12)) ] }
    Assert.Equal((ym 2022 1, None), Company.span acme)

[<Fact>]
let ``Company.isCurrent is true only when some role has no end`` () =
    let now: Company = { Name = "Now"; Dir = "now"; Logo = None; Url = "https://x.example"; Location = "X"; Roles = [ role (ym 2025 12) None ] }
    let old: Company = { Name = "Old"; Dir = "old"; Logo = None; Url = "https://x.example"; Location = "X"; Roles = [ role (ym 2020 1) (Some(ym 2021 1)) ] }
    Assert.True(Company.isCurrent now)
    Assert.False(Company.isCurrent old)

[<Fact>]
let ``Profile.careerStart is the earliest role start across companies`` () =
    let a: Company = { Name = "A"; Dir = "a"; Logo = None; Url = "https://x.example"; Location = "X"; Roles = [ role (ym 2020 6) None ] }
    let b: Company = { Name = "B"; Dir = "b"; Logo = None; Url = "https://x.example"; Location = "X"; Roles = [ role (ym 2018 8) (Some(ym 2019 4)) ] }
    let profile: Profile =
        { Name = "X"
          Headline = { Pt = ""; En = "" }
          Location = ""
          Links = []
          Companies = [ a; b ]
          Education = [] }
    Assert.Equal(ym 2018 8, Profile.careerStart profile)
