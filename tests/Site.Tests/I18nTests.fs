module I18nTests

open Xunit
open Site.Domain
open Site.I18n
open TestHelpers

[<Theory>]
[<InlineData(2025, 4, "abr 2025", "Apr 2025")>]
[<InlineData(2018, 8, "ago 2018", "Aug 2018")>]
[<InlineData(2020, 12, "dez 2020", "Dec 2020")>]
[<InlineData(2019, 5, "mai 2019", "May 2019")>]
let ``formatMonth uses short month names per language`` (year: int, month: int, pt: string, en: string) =
    Assert.Equal(pt, formatMonth Pt (ym year month))
    Assert.Equal(en, formatMonth En (ym year month))

[<Fact>]
let ``formatRange joins start and end with an en dash`` () =
    Assert.Equal("abr 2025 – dez 2025", formatRange Pt (ym 2025 4) (Some(ym 2025 12)))
    Assert.Equal("Apr 2025 – Dec 2025", formatRange En (ym 2025 4) (Some(ym 2025 12)))

[<Fact>]
let ``formatRange shows a current role as atual or Present`` () =
    Assert.Equal("dez 2025 – atual", formatRange Pt (ym 2025 12) None)
    Assert.Equal("Dec 2025 – Present", formatRange En (ym 2025 12) None)

[<Theory>]
[<InlineData(2025, 4, 2025, 12, 9)>]
[<InlineData(2021, 12, 2025, 3, 40)>]
[<InlineData(2020, 12, 2025, 12, 61)>]
[<InlineData(2025, 12, 2026, 11, 12)>]
[<InlineData(2020, 6, 2020, 6, 1)>]
let ``monthsInclusive counts both the first and the last month`` (y1: int, m1: int, y2: int, m2: int, expected: int) =
    Assert.Equal(expected, monthsInclusive (ym y1 m1) (ym y2 m2))

[<Theory>]
[<InlineData(1, "1 mês", "1 month")>]
[<InlineData(9, "9 meses", "9 months")>]
[<InlineData(12, "1 ano", "1 year")>]
[<InlineData(24, "2 anos", "2 years")>]
[<InlineData(14, "1 ano e 2 meses", "1 year 2 months")>]
[<InlineData(40, "3 anos e 4 meses", "3 years 4 months")>]
[<InlineData(61, "5 anos e 1 mês", "5 years 1 month")>]
let ``formatDuration handles singular, plural, years-only and months-only`` (months: int, pt: string, en: string) =
    Assert.Equal(pt, formatDuration Pt months)
    Assert.Equal(en, formatDuration En months)

[<Fact>]
let ``duration of a current role runs until today`` () =
    Assert.Equal("1 year", duration En (ym 2025 12) None (ym 2026 11))
    Assert.Equal("9 meses", duration Pt (ym 2025 4) (Some(ym 2025 12)) (ym 2026 11))

[<Theory>]
[<InlineData(2026, 11, 8)>]
[<InlineData(2019, 7, 0)>]
[<InlineData(2019, 8, 1)>]
let ``fullYearsBetween counts only completed years`` (year: int, month: int, expected: int) =
    Assert.Equal(expected, fullYearsBetween (ym 2018 8) (ym year month))

[<Fact>]
let ``paths and output files per language`` () =
    Assert.Equal("/", pagePath Pt)
    Assert.Equal("/en/", pagePath En)
    Assert.Equal("index.html", outputFile Pt)
    Assert.Equal("en/index.html", outputFile En)
    Assert.Equal("pt-BR", htmlLang Pt)
    Assert.Equal("en", code En)

[<Fact>]
let ``section commands follow the spec`` () =
    Assert.Equal("cat sobre.md", (strings Pt).CmdAbout)
    Assert.Equal("ls experiencia/", (strings Pt).CmdExperience)
    Assert.Equal("cat formacao.md", (strings Pt).CmdEducation)
    Assert.Equal("cat about.md", (strings En).CmdAbout)
    Assert.Equal("ls experience/", (strings En).CmdExperience)
    Assert.Equal("cat education.md", (strings En).CmdEducation)

[<Fact>]
let ``footer strings per language`` () =
    Assert.Equal("feito com", (strings Pt).BuiltWith)
    Assert.Equal("hospedado no", (strings Pt).HostedOn)
    Assert.Equal("código-fonte", (strings Pt).Source)
    Assert.Equal("built with", (strings En).BuiltWith)
    Assert.Equal("hosted on", (strings En).HostedOn)
    Assert.Equal("source", (strings En).Source)
