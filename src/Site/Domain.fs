module Site.Domain

open System

type Lang =
    | Pt
    | En

/// Text in both languages. A missing translation is a compile error.
type Localized = { Pt: string; En: string }

type YearMonth = { Year: int; Month: int }

type Role =
    { Title: Localized
      Start: YearMonth
      /// None = current role
      End: YearMonth option
      /// None = show title and dates only
      Summary: Localized option
      Tech: string list
      /// true = remote, false = on-site
      Remote: bool }

/// Dir is the folder name shown in the terminal listing, e.g. "stone".
/// Logo is a site path such as "/logos/stone.png" (None = monogram).
/// Url is the company's site; Location is where the company is based. Roles are newest first.
type Company =
    { Name: string
      Dir: string
      Logo: string option
      Url: string
      Location: string
      Roles: Role list }

type Education =
    { School: string
      Course: Localized
      StartYear: int
      EndYear: int }

type Profile =
    { Name: string
      Headline: Localized
      /// where I live, with the country spelled out per language
      Location: Localized
      /// e.g. "UTC-4"; shown after the location for remote employers
      TimeZone: string
      /// (label, url)
      Links: (string * string) list
      /// newest first
      Companies: Company list
      Education: Education list }

module Localized =
    let get (lang: Lang) (text: Localized) =
        match lang with
        | Pt -> text.Pt
        | En -> text.En

module YearMonth =
    let ofDateTime (date: DateTime) = { Year = date.Year; Month = date.Month }

module Company =
    /// True while any role at the company is ongoing.
    let isCurrent (company: Company) =
        company.Roles |> List.exists (fun r -> r.End.IsNone)

    /// Earliest role start → latest role end, or None when any role is current.
    let span (company: Company) : YearMonth * YearMonth option =
        let start = company.Roles |> List.map (fun r -> r.Start) |> List.min
        let finish =
            if company.Roles |> List.exists (fun r -> r.End.IsNone) then
                None
            else
                Some(company.Roles |> List.choose (fun r -> r.End) |> List.max)
        start, finish

module Profile =
    let careerStart (profile: Profile) =
        profile.Companies
        |> List.collect (fun c -> c.Roles)
        |> List.map (fun r -> r.Start)
        |> List.min
