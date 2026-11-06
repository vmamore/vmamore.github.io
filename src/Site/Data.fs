module Site.Data

open Site.Domain

let private t pt en : Localized = { Pt = pt; En = en }
let private same text = t text text
let private ym year month : YearMonth = { Year = year; Month = month }

let private mindbody: Company =
    { Name = "Mindbody"
      Dir = "mindbody"
      Roles =
        [ { Title = t "Engenheiro de Software" "Software Engineer"
            Start = ym 2025 12
            End = None
            Summary = None
            Tech = [] } ] }

let private stone: Company =
    { Name = "Stone"
      Dir = "stone"
      Roles =
        [ { Title = t "Engenheiro de Software Sênior" "Senior Software Engineer"
            Start = ym 2025 4
            End = Some(ym 2025 12)
            Summary =
              Some(
                  t
                      "Atuei na Antecipação de Recebíveis, produto financeiro central da Stone em que os clientes escolhem com que frequência querem antecipar suas parcelas de crédito e/ou débito. Trabalhei principalmente para manter a operação performática enquanto a base de clientes crescia, além de construir soluções para atender às regulamentações do Banco Central do Brasil."
                      "Worked on Advance Payment, a core financial product at Stone that lets clients choose how often they want to advance their credit and/or debit installments. I mainly worked on keeping operations high-performing as the customer base grew, and on building solutions to meet Central Bank of Brazil regulations."
              )
            Tech =
              [ "C#"; "ASP.NET 9"; "ASP.NET Framework 4.7"; "SQL Server"; "PostgreSQL"; "RabbitMQ"; "Redis"
                "K8S"; "Azure"; "Prometheus"; "Grafana"; "Splunk"; "OpenTelemetry"; "GitHub Actions"
                "Azure Pipelines"; "EF Core"; "Dapper"; "SpecFlow" ] }
          { Title = same "SWE III"
            Start = ym 2021 12
            End = Some(ym 2025 3)
            Summary = None
            Tech = [] }
          { Title = same "SWE II"
            Start = ym 2020 12
            End = Some(ym 2021 11)
            Summary = None
            Tech = [] } ] }

let private digix: Company =
    { Name = "Digix Soluções em TI"
      Dir = "digix"
      Roles =
        [ { Title = t "Engenheiro de Software" "Software Engineer"
            Start = ym 2020 6
            End = Some(ym 2020 11)
            Summary =
              Some(
                  t
                      "Contribuí com o Papel Zero, solução que ajuda governos a migrar processos físicos (educacionais, licitações) para o meio eletrônico."
                      "Contributed to Papel Zero, a solution that helps governments move paper-based processes (education, public bids) to digital."
              )
            Tech = [ "C# (ASP.NET MVC 4.5)"; "Aurelia"; "SQL Server"; "Azure" ] } ] }

let private db1: Company =
    { Name = "DB1 Global Software"
      Dir = "db1"
      Roles =
        [ { Title = t "Engenheiro de Software" "Software Engineer"
            Start = ym 2019 5
            End = Some(ym 2020 6)
            Summary =
              Some(
                  t
                      "Trabalhei remotamente como terceirizado em um projeto educacional nacional (SENAC), focado em dois contextos principais: Financeiro e Matrículas."
                      "Worked remotely as an outsourced developer on a national education project (SENAC), focused on two main contexts: Financial and Enrollment."
              )
            Tech = [ "C# (ASP.NET Core 2.2)"; "Angular 4"; "SQL Server"; "Azure" ] } ] }

let private imagetech: Company =
    { Name = "Grupo Imagetech"
      Dir = "imagetech"
      Roles =
        [ { Title = t "Engenheiro de Software" "Software Engineer"
            Start = ym 2018 8
            End = Some(ym 2019 4)
            Summary =
              Some(
                  t
                      "Minha primeira experiência como desenvolvedor em equipe. Contribuí com o Dívida Ativa, projeto jurídico do Estado de Mato Grosso do Sul."
                      "My first experience working as a developer on a team. Contributed to Dívida Ativa, a legal project of the State of Mato Grosso do Sul."
              )
            Tech = [ "C# (ASP.NET MVC 4.5)"; "jQuery"; "Bootstrap"; "SQL Server"; "Azure" ] } ] }

let profile: Profile =
    { Name = "Vinicius Mamoré"
      Headline = t "Engenheiro de Software @ Mindbody" "Software Engineer @ Mindbody"
      Location = "Campo Grande, MS"
      Links =
        [ "GitHub", "https://github.com/vmamore"
          "LinkedIn", "https://www.linkedin.com/in/vinicius-mamor%C3%A9-ab180b142/" ]
      Companies = [ mindbody; stone; digix; db1; imagetech ]
      Education =
        [ { School = "Universidade Federal de Mato Grosso do Sul"
            Course = t "Sistemas de Informação" "Information Systems"
            StartYear = 2017
            EndYear = 2023 } ] }
