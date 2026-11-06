module Site.I18n

open Site.Domain

let code lang =
    match lang with
    | Pt -> "pt"
    | En -> "en"

let htmlLang lang =
    match lang with
    | Pt -> "pt-BR"
    | En -> "en"

let pagePath lang =
    match lang with
    | Pt -> "/"
    | En -> "/en/"

let outputFile lang =
    match lang with
    | Pt -> "index.html"
    | En -> "en/index.html"

type Strings =
    { CmdAbout: string
      CmdExperience: string
      CmdEducation: string
      Description: string
      BuiltWith: string
      HostedOn: string
      Source: string
      CurrentTag: string }

let strings lang =
    match lang with
    | Pt ->
        { CmdAbout = "cat sobre.md"
          CmdExperience = "ls experiencia/"
          CmdEducation = "cat formacao.md"
          Description = "Vinicius Mamoré, engenheiro de software backend: sobre mim e experiência profissional."
          BuiltWith = "feito com"
          HostedOn = "hospedado no"
          Source = "código-fonte"
          CurrentTag = "atual" }
    | En ->
        { CmdAbout = "cat about.md"
          CmdExperience = "ls experience/"
          CmdEducation = "cat education.md"
          Description = "Vinicius Mamoré, backend software engineer: about me and work experience."
          BuiltWith = "built with"
          HostedOn = "hosted on"
          Source = "source"
          CurrentTag = "current" }

let private monthsPt =
    [| "jan"; "fev"; "mar"; "abr"; "mai"; "jun"; "jul"; "ago"; "set"; "out"; "nov"; "dez" |]

let private monthsEn =
    [| "Jan"; "Feb"; "Mar"; "Apr"; "May"; "Jun"; "Jul"; "Aug"; "Sep"; "Oct"; "Nov"; "Dec" |]

let formatMonth lang (date: YearMonth) =
    let names =
        match lang with
        | Pt -> monthsPt
        | En -> monthsEn
    sprintf "%s %d" names.[date.Month - 1] date.Year

let private present lang =
    match lang with
    | Pt -> "atual"
    | En -> "Present"

let formatRange lang (start: YearMonth) (finish: YearMonth option) =
    let last =
        match finish with
        | Some f -> formatMonth lang f
        | None -> present lang
    sprintf "%s – %s" (formatMonth lang start) last

/// Months from start to finish, counting both ends (LinkedIn's convention).
let monthsInclusive (start: YearMonth) (finish: YearMonth) =
    (finish.Year - start.Year) * 12 + (finish.Month - start.Month) + 1

let formatDuration lang (totalMonths: int) =
    let years, months = totalMonths / 12, totalMonths % 12
    match lang with
    | Pt ->
        let y = if years = 1 then "1 ano" else sprintf "%d anos" years
        let m = if months = 1 then "1 mês" else sprintf "%d meses" months
        match years, months with
        | 0, _ -> m
        | _, 0 -> y
        | _ -> sprintf "%s e %s" y m
    | En ->
        let y = if years = 1 then "1 year" else sprintf "%d years" years
        let m = if months = 1 then "1 month" else sprintf "%d months" months
        match years, months with
        | 0, _ -> m
        | _, 0 -> y
        | _ -> sprintf "%s %s" y m

/// Duration text for a role or company; a current one (None) ends at `today`.
let duration lang (start: YearMonth) (finish: YearMonth option) (today: YearMonth) =
    formatDuration lang (monthsInclusive start (defaultArg finish today))

/// Completed years from start to today (used for "{{years}} years of experience").
let fullYearsBetween (start: YearMonth) (today: YearMonth) =
    ((today.Year - start.Year) * 12 + (today.Month - start.Month)) / 12
