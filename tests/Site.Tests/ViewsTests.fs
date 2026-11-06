module ViewsTests

open Xunit
open Site.Domain
open Site.Views
open TestHelpers

let private role title start finish summary tech : Role =
    { Title = { Pt = title + " pt"; En = title + " en" }
      Start = start
      End = finish
      Summary = summary
      Tech = tech }

let private acme: Company =
    { Name = "Acme Ltda"
      Dir = "acme"
      Logo = Some "/logos/acme.png"
      Roles =
        [ role "Senior" (ym 2024 1) None (Some { Pt = "Resumo"; En = "Summary" }) [ "F#"; "SQL" ]
          role "Junior" (ym 2022 1) (Some(ym 2023 12)) None [] ] }

let private solo: Company =
    { Name = "Solo"
      Dir = "solo"
      Logo = None
      Roles = [ role "Dev" (ym 2020 1) (Some(ym 2021 6)) None [] ] }

let private sample: Profile =
    { Name = "Ana Teste"
      Headline = { Pt = "Engenheira"; En = "Engineer" }
      Location = "Campo Grande, MS"
      Links = [ "GitHub", "https://github.com/ana" ]
      Companies = [ acme; solo ]
      Education =
        [ { School = "UFMS"
            Course = { Pt = "Sistemas de Informação"; En = "Information Systems" }
            StartYear = 2017
            EndYear = 2023 } ] }

/// External links open in a new tab.
let private ext (url: string) (label: string) =
    sprintf "<a href=\"%s\" target=\"_blank\" rel=\"noopener noreferrer\">%s</a>" url label

let private renderFor lang =
    render
        { Lang = lang
          Profile = sample
          AboutHtml = "<p>Olá <em>mundo</em></p>"
          Today = ym 2026 11 }

[<Fact>]
let ``escape only touches the characters HTML needs`` () =
    Assert.Equal("a &lt;b&gt; &amp; &quot;c&quot; ção", escape "a <b> & \"c\" ção")

[<Fact>]
let ``pt page declares its language and links to en`` () =
    let html = renderFor Pt
    contains "<!DOCTYPE html>" html
    contains "<html lang=\"pt-BR\">" html
    contains "<span class=\"current\" aria-current=\"page\">pt</span>" html
    contains "<a href=\"/en/\" hreflang=\"en\" lang=\"en\">en</a>" html

[<Fact>]
let ``en page declares its language and links to pt`` () =
    let html = renderFor En
    contains "<html lang=\"en\">" html
    contains "<span class=\"current\" aria-current=\"page\">en</span>" html
    contains "<a href=\"/\" hreflang=\"pt-BR\" lang=\"pt-BR\">pt</a>" html

[<Fact>]
let ``head has canonical and hreflang alternates`` () =
    let html = renderFor En
    contains "<link rel=\"canonical\" href=\"https://vmamore.com.br/en/\">" html
    contains "<link rel=\"alternate\" hreflang=\"pt-BR\" href=\"https://vmamore.com.br/\">" html
    contains "<link rel=\"alternate\" hreflang=\"en\" href=\"https://vmamore.com.br/en/\">" html
    contains "<link rel=\"alternate\" hreflang=\"x-default\" href=\"https://vmamore.com.br/\">" html
    contains "<link rel=\"stylesheet\" href=\"/style.css\">" html

[<Fact>]
let ``sections are headed by shell commands in the page language`` () =
    let pt = renderFor Pt
    contains "<h2 class=\"cmd\">cat sobre.md</h2>" pt
    contains "<h2 class=\"cmd\">ls experiencia/</h2>" pt
    contains "<h2 class=\"cmd\">cat formacao.md</h2>" pt
    let en = renderFor En
    contains "<h2 class=\"cmd\">ls experience/</h2>" en

[<Fact>]
let ``whoami block shows name, headline, location and lowercase links`` () =
    let html = renderFor Pt
    contains "<h1 class=\"name\">Ana Teste</h1>" html
    contains "Engenheira · Campo Grande, MS" html
    contains (ext "https://github.com/ana" "github") html

[<Fact>]
let ``about html is inserted as-is`` () =
    contains "<p>Olá <em>mundo</em></p>" (renderFor Pt)

[<Fact>]
let ``multi-role company shows its dir, full name and span`` () =
    let html = renderFor Pt
    contains "<strong title=\"Acme Ltda\">acme/</strong>" html
    contains "jan 2022 – atual · 4 anos e 11 meses" html

[<Fact>]
let ``current company is tagged and past companies are marked as past`` () =
    let pt = renderFor Pt
    contains "<li class=\"company current\">" pt
    contains "<li class=\"company past\">" pt
    contains "<strong title=\"Acme Ltda\">acme/</strong><span class=\"tag\">● atual</span>" pt
    Assert.Equal(1, count "class=\"tag\"" pt)
    contains "<span class=\"tag\">● current</span>" (renderFor En)

[<Fact>]
let ``each company shows its logo, or a monogram when it has none`` () =
    let pt = renderFor Pt
    contains "<img class=\"logo\" src=\"/logos/acme.png\" alt=\"\" width=\"18\" height=\"18\"><strong title=\"Acme Ltda\">" pt
    contains "<span class=\"logo monogram\" aria-hidden=\"true\">S</span><strong title=\"Solo\">" pt

[<Fact>]
let ``single-role company does not repeat the role dates`` () =
    let html = renderFor Pt
    contains "<strong title=\"Solo\">solo/</strong>" html
    Assert.Equal(1, count "jan 2020 – jun 2021 · 1 ano e 6 meses" html)

[<Fact>]
let ``roles use tree glyphs with the last one closing the branch`` () =
    let html = renderFor Pt
    Assert.Equal(1, count "├─" html)
    Assert.Equal(2, count "└─" html)

[<Fact>]
let ``summary and tech appear only when present`` () =
    let html = renderFor En
    Assert.Equal(1, count "class=\"summary\"" html)
    contains "<p class=\"summary\">Summary</p>" html
    Assert.Equal(1, count "class=\"tech\"" html)
    contains "<p class=\"tech\">F#, SQL</p>" html

[<Fact>]
let ``accented text is not entity-encoded`` () =
    let html = renderFor Pt
    contains "Sistemas de Informação · 2017 – 2023" html
    notContains "&#231;" html

[<Fact>]
let ``footer credits the stack, the host and links the source`` () =
    let pt = renderFor Pt
    contains "© 2026 Ana Teste" pt
    contains
        ("feito com "
         + ext "https://fsharp.org" "F#" + ", "
         + ext "https://github.com/Lanayx/Oxpecker" "Oxpecker.ViewEngine" + ", "
         + ext "https://github.com/xoofx/markdig" "Markdig" + ", "
         + ext "https://www.jetbrains.com/lp/mono/" "JetBrains Mono")
        pt
    contains ("hospedado no " + ext "https://pages.github.com" "GitHub Pages") pt
    contains (ext "https://github.com/vmamore/vmamore.github.io" "código-fonte") pt
    let en = renderFor En
    contains ("built with " + ext "https://fsharp.org" "F#") en
    contains ("hosted on " + ext "https://pages.github.com" "GitHub Pages") en
    contains (ext "https://github.com/vmamore/vmamore.github.io" "source") en

[<Fact>]
let ``only external links open in a new tab`` () =
    let html = renderFor Pt
    // 1 profile link + 4 stack links + GitHub Pages + source
    Assert.Equal(7, count "target=\"_blank\"" html)
    Assert.Equal(7, count "rel=\"noopener noreferrer\"" html)
    contains "<a href=\"/en/\" hreflang=\"en\" lang=\"en\">en</a>" html

[<Fact>]
let ``page ends with the caret and has no email`` () =
    let html = renderFor Pt
    contains "<span class=\"caret\"></span>" html
    notContains "mailto:" html
