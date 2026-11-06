module Site.Server

open System
open System.IO
open System.Net

let contentType (path: string) =
    match Path.GetExtension(path).ToLowerInvariant() with
    | ".html" -> "text/html; charset=utf-8"
    | ".css" -> "text/css; charset=utf-8"
    | ".svg" -> "image/svg+xml"
    | ".woff2" -> "font/woff2"
    | ".png" -> "image/png"
    | ".txt" -> "text/plain; charset=utf-8"
    | _ -> "application/octet-stream"

/// Maps a URL path to a file inside outDir. Folders map to their index.html.
let resolve (outDir: string) (urlPath: string) : string option =
    let root = Path.GetFullPath(outDir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
    let relative = Uri.UnescapeDataString(urlPath).TrimStart('/')
    let candidate = Path.GetFullPath(Path.Combine(root, relative))
    let inside =
        candidate.Equals(root, StringComparison.OrdinalIgnoreCase)
        || candidate.StartsWith(root + string Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
    if not inside then
        None
    elif Directory.Exists candidate then
        let index = Path.Combine(candidate, "index.html")
        if File.Exists index then Some index else None
    elif File.Exists candidate then
        Some candidate
    else
        None

let serve (outDir: string) (port: int) =
    use listener = new HttpListener()
    listener.Prefixes.Add(sprintf "http://localhost:%d/" port)
    listener.Start()
    printfn "Serving %s at http://localhost:%d/ (Ctrl+C to stop)" outDir port
    while true do
        let context = listener.GetContext()
        let response = context.Response
        match resolve outDir context.Request.Url.AbsolutePath with
        | Some file ->
            let bytes = File.ReadAllBytes file
            response.ContentType <- contentType file
            response.ContentLength64 <- int64 bytes.Length
            response.OutputStream.Write(bytes, 0, bytes.Length)
        | None -> response.StatusCode <- 404
        response.Close()
