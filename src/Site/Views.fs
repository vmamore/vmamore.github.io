module Site.Views

open Oxpecker.ViewEngine
open Oxpecker.ViewEngine.Aria
open Site.Domain
open Site.I18n

let baseUrl = "https://vmamore.com.br"

let sourceUrl = "https://github.com/vmamore/vmamore.github.io"

type PageModel =
    { Lang: Lang
      Profile: Profile
      AboutHtml: string
      Today: YearMonth }

/// Escapes only what HTML requires. Oxpecker's text nodes also turn "ç" into "&#231;",
/// which renders fine but makes the source unreadable.
let escape (s: string) =
    s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;")

let text (s: string) = raw (escape s)

/// A link to another site: opens in a new tab without giving it access to this page.
let externalLink (url: string) (label: string) =
    a(href = url, target = "_blank", rel = "noopener noreferrer") { text label }

let private dateLine lang today (start: YearMonth) (finish: YearMonth option) =
    sprintf "%s · %s" (formatRange lang start finish) (duration lang start finish today)

let private roleView lang today (glyph: string) (role: Role) =
    li(class' = "role") {
        span(class' = "branch", ariaHidden = true) { text glyph }
        div(class' = "role-body") {
            p(class' = "role-head") {
                span(class' = "role-title") { text (Localized.get lang role.Title) }
                span(class' = "dim") { text (dateLine lang today role.Start role.End) }
            }
            match role.Summary with
            | Some summary -> p(class' = "summary") { text (Localized.get lang summary) }
            | None -> ()
            if not role.Tech.IsEmpty then
                p(class' = "tech") { text (String.concat ", " role.Tech) }
        }
    }

let private companyView lang today (company: Company) =
    let start, finish = Company.span company
    let lastIndex = company.Roles.Length - 1
    let isCurrent = Company.isCurrent company
    li(class' = (if isCurrent then "company current" else "company past")) {
        p(class' = "company-head") {
            strong(title = company.Name) { text (company.Dir + "/") }
            if isCurrent then
                span(class' = "tag") { text ("● " + (strings lang).CurrentTag) }
            if company.Roles.Length > 1 then
                span(class' = "dim") { text (dateLine lang today start finish) }
        }
        ol(class' = "roles") {
            for i, role in List.indexed company.Roles do
                roleView lang today (if i = lastIndex then "└─" else "├─") role
        }
    }

let private educationView lang (e: Education) =
    li() {
        strong() { text e.School }
        span(class' = "dim") { text (sprintf "%s · %d – %d" (Localized.get lang e.Course) e.StartYear e.EndYear) }
    }

let private langSwitch current =
    let item lang : HtmlElement =
        if lang = current then
            span(class' = "current").attr("aria-current", "page") { text (code lang) }
        else
            a(href = pagePath lang, hreflang = htmlLang lang, lang = htmlLang lang) { text (code lang) }
    nav(class' = "lang", ariaLabel = "Language / Idioma") {
        item Pt
        text " / "
        item En
    }

let private stack =
    [ "F#", "https://fsharp.org"
      "Oxpecker.ViewEngine", "https://github.com/Lanayx/Oxpecker"
      "Markdig", "https://github.com/xoofx/markdig"
      "JetBrains Mono", "https://www.jetbrains.com/lp/mono/" ]

let private footerView lang (today: YearMonth) (name: string) =
    let s = strings lang
    footer() {
        p() { text (sprintf "© %d %s" today.Year name) }
        p(class' = "built") {
            text (s.BuiltWith + " ")
            for i, (label, url) in List.indexed stack do
                if i > 0 then text ", "
                externalLink url label
            text (sprintf " · %s " s.HostedOn)
            externalLink "https://pages.github.com" "GitHub Pages"
            text " · "
            externalLink sourceUrl s.Source
        }
    }

let private alternate (hreflang': string) (path: string) =
    link(rel = "alternate", hreflang = hreflang', href = baseUrl + path)

let page (model: PageModel) =
    let lang = model.Lang
    let prof = model.Profile
    let s = strings lang
    html(lang = htmlLang lang) {
        head() {
            meta(charset = "utf-8")
            meta(name = "viewport", content = "width=device-width, initial-scale=1")
            title() { text (sprintf "%s · %s" prof.Name (Localized.get lang prof.Headline)) }
            meta(name = "description", content = s.Description)
            meta(name = "color-scheme", content = "dark")
            link(rel = "canonical", href = baseUrl + pagePath lang)
            alternate "pt-BR" (pagePath Pt)
            alternate "en" (pagePath En)
            alternate "x-default" (pagePath Pt)
            link(rel = "icon", type' = "image/svg+xml", href = "/favicon.svg")
            link(rel = "preload", href = "/fonts/JetBrainsMono-Regular.woff2", type' = "font/woff2")
                .attr("as", "font")
                .attr("crossorigin", "")
            link(rel = "stylesheet", href = "/style.css")
        }
        body() {
            div(class' = "page") {
                header(class' = "top") { langSwitch lang }
                main() {
                    section(id = "whoami") {
                        p(class' = "cmd", ariaHidden = true) { text "whoami" }
                        h1(class' = "name") { text prof.Name }
                        p(class' = "dim") { text (sprintf "%s · %s" (Localized.get lang prof.Headline) prof.Location) }
                        p(class' = "links") {
                            for label, url in prof.Links do
                                externalLink url (label.ToLowerInvariant())
                        }
                    }
                    section(id = "about") {
                        h2(class' = "cmd") { text s.CmdAbout }
                        div(class' = "prose") { raw model.AboutHtml }
                    }
                    section(id = "experience") {
                        h2(class' = "cmd") { text s.CmdExperience }
                        ol(class' = "companies") {
                            for company in prof.Companies do
                                companyView lang model.Today company
                        }
                    }
                    section(id = "education") {
                        h2(class' = "cmd") { text s.CmdEducation }
                        ul(class' = "education") {
                            for e in prof.Education do
                                educationView lang e
                        }
                    }
                    p(class' = "cmd", ariaHidden = true) { span(class' = "caret") }
                }
                footerView lang model.Today prof.Name
            }
        }
    }

let render (model: PageModel) = page model |> Render.toHtmlDocString
