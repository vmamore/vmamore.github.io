# vmamore.com.br

Personal site of Vinicius Mamoré. Built with a small F# static site generator
(Oxpecker.ViewEngine + Markdig) and deployed to GitHub Pages.

## Run locally

```pwsh
dotnet test
dotnet run --project src/Site -- serve   # http://localhost:5000
```

`dotnet run --project src/Site -- build` writes the site to `_site/`.

## Edit content

- Experience, headline, links, education: `src/Site/Data.fs` (every text has `Pt` and `En`).
- About text: `content/about.pt.md` and `content/about.en.md`. `{{years}}` becomes the years since the first job.
- Styles: `static/style.css`.

Pushing to `main` runs the tests and deploys.

## Domain (registro.br)

1. Register `vmamore.com.br` at registro.br.
2. In the domain's DNS zone ("Editar zona"), add:
   - `A` `@` → `185.199.108.153`, `185.199.109.153`, `185.199.110.153`, `185.199.111.153`
   - `CNAME` `www` → `vmamore.github.io`
3. In GitHub: Settings → Pages → Custom domain → `vmamore.com.br`, then tick "Enforce HTTPS" once the certificate is ready.
4. Recommended: verify the domain under your GitHub account Settings → Pages.

## License

Content © Vinicius Mamoré. JetBrains Mono is under the SIL Open Font License (`static/fonts/OFL.txt`).
