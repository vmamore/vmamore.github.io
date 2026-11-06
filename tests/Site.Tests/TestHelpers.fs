module TestHelpers

open System
open System.IO
open Xunit
open Site.Domain

let ym year month : YearMonth = { Year = year; Month = month }

let contains (needle: string) (haystack: string) =
    Assert.True(haystack.Contains(needle, StringComparison.Ordinal), sprintf "Expected to find: %s" needle)

let notContains (needle: string) (haystack: string) =
    Assert.False(haystack.Contains(needle, StringComparison.Ordinal), sprintf "Did not expect to find: %s" needle)

let count (needle: string) (haystack: string) =
    let rec go (from: int) acc =
        match haystack.IndexOf(needle, from, StringComparison.Ordinal) with
        | -1 -> acc
        | i -> go (i + needle.Length) (acc + 1)
    go 0 0

/// Creates a fresh empty directory under the OS temp folder.
let tempDir () =
    let dir = Path.Combine(Path.GetTempPath(), "vmamore-tests-" + Guid.NewGuid().ToString("N"))
    Directory.CreateDirectory dir |> ignore
    dir
