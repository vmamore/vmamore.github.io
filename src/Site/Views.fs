module Site.Views

open Giraffe.ViewEngine
open Site.Domain
open Site.I18n

let baseUrl = "https://vmamore.com.br"

type PageModel =
    { Lang: Lang
      Profile: Profile
      AboutHtml: string
      Today: YearMonth }

/// Escapes only what HTML requires. Giraffe's `str` also turns "ç" into "&#231;",
/// which renders fine but makes the source unreadable.
let escape (s: string) =
    s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;")

let text (s: string) = rawText (escape s)

/// A link to another site: opens in a new tab without giving it access to this page.
let externalLink (url: string) (label: string) =
    a [ _href url; attr "target" "_blank"; attr "rel" "noopener noreferrer" ] [ text label ]

let private dateLine lang today (start: YearMonth) (finish: YearMonth option) =
    sprintf "%s · %s" (formatRange lang start finish) (duration lang start finish today)

let private roleView lang today (glyph: string) (role: Role) =
    let head =
        p [ _class "role-head" ] [
            span [ _class "role-title" ] [ text (Localized.get lang role.Title) ]
            span [ _class "dim" ] [ text (dateLine lang today role.Start role.End) ]
        ]
    let summary =
        role.Summary
        |> Option.map (fun s -> p [ _class "summary" ] [ text (Localized.get lang s) ])
        |> Option.toList
    let tech =
        if List.isEmpty role.Tech then []
        else [ p [ _class "tech" ] [ text (String.concat ", " role.Tech) ] ]
    li [ _class "role" ] [
        span [ _class "branch"; attr "aria-hidden" "true" ] [ text glyph ]
        div [ _class "role-body" ] ([ head ] @ summary @ tech)
    ]

let private companyView lang today (company: Company) =
    let start, finish = Company.span company
    let lastIndex = List.length company.Roles - 1
    let spanLine =
        if List.length company.Roles > 1 then
            [ span [ _class "dim" ] [ text (dateLine lang today start finish) ] ]
        else
            []
    li [ _class "company" ] [
        p [ _class "company-head" ] ([ strong [ attr "title" company.Name ] [ text (company.Dir + "/") ] ] @ spanLine)
        ol [ _class "roles" ] (
            company.Roles
            |> List.mapi (fun i r -> roleView lang today (if i = lastIndex then "└─" else "├─") r)
        )
    ]

let private educationView lang (e: Education) =
    li [] [
        strong [] [ text e.School ]
        span [ _class "dim" ] [ text (sprintf "%s · %d – %d" (Localized.get lang e.Course) e.StartYear e.EndYear) ]
    ]

let private langSwitch current =
    let item lang =
        if lang = current then
            span [ _class "current"; attr "aria-current" "page" ] [ text (code lang) ]
        else
            a [ _href (pagePath lang); attr "hreflang" (htmlLang lang); attr "lang" (htmlLang lang) ] [ text (code lang) ]
    nav [ _class "lang"; attr "aria-label" "Language / Idioma" ] [ item Pt; text " / "; item En ]

let sourceUrl = "https://github.com/vmamore/vmamore.github.io"

let private stack =
    [ "F#", "https://fsharp.org"
      "Giraffe.ViewEngine", "https://github.com/giraffe-fsharp/Giraffe.ViewEngine"
      "Markdig", "https://github.com/xoofx/markdig"
      "JetBrains Mono", "https://www.jetbrains.com/lp/mono/" ]

/// Puts `separator` between nodes: [a; b; c] → [a; sep; b; sep; c].
let private joinWith (separator: XmlNode) (nodes: XmlNode list) =
    nodes |> List.mapi (fun i n -> if i = 0 then [ n ] else [ separator; n ]) |> List.concat

let private footerView lang (today: YearMonth) (name: string) =
    let s = strings lang
    let stackLinks = stack |> List.map (fun (label, url) -> externalLink url label)
    footer [] [
        p [] [ text (sprintf "© %d %s" today.Year name) ]
        p [ _class "built" ] (
            [ text (s.BuiltWith + " ") ]
            @ joinWith (text ", ") stackLinks
            @ [ text (sprintf " · %s " s.HostedOn)
                externalLink "https://pages.github.com" "GitHub Pages"
                text " · "
                externalLink sourceUrl s.Source ]
        )
    ]

let private alternate (hreflang: string) (path: string) =
    link [ _rel "alternate"; attr "hreflang" hreflang; _href (baseUrl + path) ]

let page (model: PageModel) =
    let lang = model.Lang
    let prof = model.Profile
    let s = strings lang
    html [ attr "lang" (htmlLang lang) ] [
        head [] [
            meta [ _charset "utf-8" ]
            meta [ _name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [ text (sprintf "%s · %s" prof.Name (Localized.get lang prof.Headline)) ]
            meta [ _name "description"; _content s.Description ]
            meta [ _name "color-scheme"; _content "dark" ]
            link [ _rel "canonical"; _href (baseUrl + pagePath lang) ]
            alternate "pt-BR" (pagePath Pt)
            alternate "en" (pagePath En)
            alternate "x-default" (pagePath Pt)
            link [ _rel "icon"; _type "image/svg+xml"; _href "/favicon.svg" ]
            link [ _rel "preload"; _href "/fonts/JetBrainsMono-Regular.woff2"; attr "as" "font"; _type "font/woff2"; attr "crossorigin" "" ]
            link [ _rel "stylesheet"; _href "/style.css" ]
        ]
        body [] [
            div [ _class "page" ] [
                header [ _class "top" ] [ langSwitch lang ]
                main [] [
                    section [ _id "whoami" ] [
                        p [ _class "cmd"; attr "aria-hidden" "true" ] [ text "whoami" ]
                        h1 [ _class "name" ] [ text prof.Name ]
                        p [ _class "dim" ] [ text (sprintf "%s · %s" (Localized.get lang prof.Headline) prof.Location) ]
                        p [ _class "links" ] (
                            prof.Links |> List.map (fun (label, url) -> externalLink url (label.ToLowerInvariant()))
                        )
                    ]
                    section [ _id "about" ] [
                        h2 [ _class "cmd" ] [ text s.CmdAbout ]
                        div [ _class "prose" ] [ rawText model.AboutHtml ]
                    ]
                    section [ _id "experience" ] [
                        h2 [ _class "cmd" ] [ text s.CmdExperience ]
                        ol [ _class "companies" ] (prof.Companies |> List.map (companyView lang model.Today))
                    ]
                    section [ _id "education" ] [
                        h2 [ _class "cmd" ] [ text s.CmdEducation ]
                        ul [ _class "education" ] (prof.Education |> List.map (educationView lang))
                    ]
                    p [ _class "cmd"; attr "aria-hidden" "true" ] [ span [ _class "caret" ] [] ]
                ]
                footerView lang model.Today prof.Name
            ]
        ]
    ]

let render (model: PageModel) = RenderView.AsString.htmlDocument (page model)
