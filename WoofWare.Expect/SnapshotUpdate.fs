namespace WoofWare.Expect

open System
open System.Text.RegularExpressions

type private StringLiteralInfo =
    {
        StartLine : int
        StartColumn : int
        EndLine : int
        EndColumn : int
        Content : string
    }

    override this.ToString () =
        sprintf "%i:%i to %i:%i: %s" this.StartLine this.StartColumn this.EndLine this.EndColumn this.Content

type private Position =
    {
        Line : int
        Column : int
        TotalOffset : int
    }

[<RequireQualifiedAccess>]
module internal SnapshotUpdate =
    /// Convert a string position to line/column
    let private positionToLineColumn (text : string) (offset : int) : Position =
        let rec loop (line : int) (col : int) (totalOffset : int) (i : int) : Position =
            if i >= text.Length || totalOffset = offset then
                {
                    Line = line
                    Column = col
                    TotalOffset = totalOffset
                }
            elif text.[i] = '\n' then
                loop (line + 1) 0 (totalOffset + 1) (i + 1)
            else
                loop line (col + 1) (totalOffset + 1) (i + 1)

        loop 0 0 0 0

    /// Skip whitespace and comments, returning the position after them
    let rec private skipWhitespaceAndComments (text : string) (startPos : int) : int option =
        let rec skipComment (depth : int) (pos : int) : int option =
            if pos >= text.Length - 1 then
                None
            elif pos + 1 < text.Length && text.[pos] = '(' && text.[pos + 1] = '*' then
                skipComment (depth + 1) (pos + 2)
            elif pos + 1 < text.Length && text.[pos] = '*' && text.[pos + 1] = ')' then
                if depth = 1 then
                    Some (pos + 2)
                else
                    skipComment (depth - 1) (pos + 2)
            else
                skipComment depth (pos + 1)

        let rec loop pos =
            if pos >= text.Length then
                None
            elif pos + 1 < text.Length && text.[pos] = '(' && text.[pos + 1] = '*' then
                skipComment 1 (pos + 2) |> Option.bind loop
            elif Char.IsWhiteSpace (text.[pos]) then
                loop (pos + 1)
            else
                Some pos

        loop startPos

    /// Parse a regular string literal
    let private parseRegularString (text : string) (startPos : int) : (string * int) option =
        let rec loop pos content escaped =
            if pos >= text.Length then
                None
            elif escaped then
                let unescaped =
                    match text.[pos] with
                    | 'n' -> "\n"
                    | 'r' -> "\r"
                    | 't' -> "\t"
                    | '\\' -> "\\"
                    | '"' -> "\""
                    | c -> string c

                loop (pos + 1) (content + unescaped) false
            elif text.[pos] = '\\' then
                loop (pos + 1) content true
            elif text.[pos] = '"' then
                Some (content, pos + 1)
            else
                loop (pos + 1) (content + string text.[pos]) false

        loop (startPos + 1) "" false

    /// Parse a verbatim string literal (@"...")
    let private parseVerbatimString (text : string) (startPos : int) : (string * int) option =
        let rec loop pos content =
            if pos >= text.Length then
                None
            elif pos + 1 < text.Length && text.[pos] = '"' && text.[pos + 1] = '"' then
                // Escaped quote in verbatim string
                loop (pos + 2) (content + "\"")
            elif text.[pos] = '"' then
                // End of string
                Some (content, pos + 1)
            else
                loop (pos + 1) (content + string text.[pos])

        // Skip the @" prefix
        loop (startPos + 2) ""

    /// Parse a triple-quoted string literal
    let private parseTripleQuotedString (text : string) (startPos : int) : (string * int) option =
        // startPos points to the first "
        if
            startPos + 2 >= text.Length
            || text.[startPos] <> '"'
            || text.[startPos + 1] <> '"'
            || text.[startPos + 2] <> '"'
        then
            None
        else
            let contentStart = startPos + 3
            let closePos = text.IndexOf ("\"\"\"", contentStart)

            if closePos = -1 then
                None
            else
                let content = text.Substring (contentStart, closePos - contentStart)
                Some (content, closePos + 3)

    /// Find the string literal after a snapshot keyword
    let private findSnapshotString (lines : string[]) (snapshotLine : int) : StringLiteralInfo option =
        let startIdx = snapshotLine - 1

        if startIdx >= lines.Length then
            None
        else
            // We need to include enough lines to capture multi-line strings
            // Take a reasonable number of lines after the snapshot line
            let maxLines = min 50 (lines.Length - startIdx)
            let relevantLines = lines |> Array.skip startIdx |> Array.take maxLines

            let searchText = String.concat "\n" relevantLines

            // Find snapshot keyword
            let snapshotMatch = Regex.Match (searchText, @"\b(snapshot|snapshotJson)\b")

            if not snapshotMatch.Success then
                None
            else
                // Work with positions relative to searchText throughout
                let snapshotEnd = snapshotMatch.Index + snapshotMatch.Length

                // Skip whitespace and comments after "snapshot"
                skipWhitespaceAndComments searchText snapshotEnd
                |> Option.bind (fun stringStart ->
                    if stringStart >= searchText.Length then
                        None
                    else
                        // Check what type of string literal we have
                        let parseResult =
                            if
                                stringStart + 2 < searchText.Length
                                && searchText.[stringStart] = '"'
                                && searchText.[stringStart + 1] = '"'
                                && searchText.[stringStart + 2] = '"'
                            then
                                // Triple-quoted string
                                parseTripleQuotedString searchText stringStart
                                |> Option.map (fun (content, endPos) -> (content, stringStart, endPos))
                            elif
                                stringStart + 1 < searchText.Length
                                && searchText.[stringStart] = '@'
                                && searchText.[stringStart + 1] = '"'
                            then
                                // Verbatim string
                                parseVerbatimString searchText stringStart
                                |> Option.map (fun (content, endPos) -> (content, stringStart, endPos))
                            elif searchText.[stringStart] = '"' then
                                // Regular string
                                parseRegularString searchText stringStart
                                |> Option.map (fun (content, endPos) -> (content, stringStart, endPos))
                            else
                                None

                        parseResult
                        |> Option.map (fun (content, stringStartPos, stringEndPos) ->
                            let startPos = positionToLineColumn searchText stringStartPos
                            let endPos = positionToLineColumn searchText stringEndPos

                            {
                                StartLine = startIdx + startPos.Line + 1
                                StartColumn = startPos.Column
                                EndLine = startIdx + endPos.Line + 1
                                EndColumn = endPos.Column
                                Content = content
                            }
                        )
                )

    /// Update the snapshot string with a new value; this doesn't edit the file on disk, but instead returns the new contents.
    /// We always write verbatim strings (@"...") or triple-quoted strings for simplicity
    let private updateSnapshot (lines : string[]) (info : StringLiteralInfo) (newContent : string) : string[] =
        // Choose the best string format based on content
        let newString =
            if newContent.Contains ("\"\"\"") then
                // Content has triple quotes - must use verbatim string
                "@\"" + newContent.Replace ("\"", "\"\"") + "\""
            elif newContent.IndexOf '\n' >= 0 then
                // Multi-line content - use triple quotes for readability
                "\"\"\"" + newContent + "\"\"\""
            else
                // Single-line content - use verbatim string
                "@\"" + newContent.Replace ("\"", "\"\"") + "\""

        if info.StartLine = info.EndLine then
            // Single line update
            lines
            |> Array.mapi (fun i line ->
                if i = info.StartLine - 1 then
                    let before = line.Substring (0, info.StartColumn)
                    let after = line.Substring (info.EndColumn)
                    before + newString + after
                else
                    line
            )
        else
            // Multi-line update
            let startLineIdx = info.StartLine - 1
            let endLineIdx = info.EndLine - 1

            let before = lines.[startLineIdx].Substring (0, info.StartColumn)
            let after = lines.[endLineIdx].Substring (info.EndColumn)

            let newLines =
                if newContent.IndexOf '\n' >= 0 then
                    // Keep as triple-quoted
                    [|
                        yield before + "\"\"\""
                        yield! newContent.Split ('\n')
                        yield "\"\"\"" + after
                    |]
                else
                    // Convert to single-line verbatim string
                    [| before + "@\"" + newContent.Replace ("\"", "\"\"") + "\"" + after |]

            [|
                yield! lines |> Array.take startLineIdx
                yield! newLines
                yield! lines |> Array.skip (endLineIdx + 1)
            |]

    /// <remarks>Example usage:
    ///   <c>updateSnapshotAtLine [|lines-of-file|] 42 "new test output"</c>
    ///
    /// This will find a snapshot call on line 42 like:
    ///   snapshot "old value"       -> snapshot @"new test output"
    ///   snapshot @"old value"      -> snapshot @"new test output"
    ///   snapshot """old value"""   -> snapshot @"new test output"
    ///   snapshot """multi
    ///               line"""       -> snapshot """multi
    ///                                            line"""
    ///   snapshot "has \"\"\" in it" -> snapshot @"has """""" in it"
    /// </remarks>
    let updateSnapshotAtLine (fileLines : string[]) (snapshotLine : int) (newValue : string) : string[] =
        match findSnapshotString fileLines snapshotLine with
        | Some info ->
            Console.Error.WriteLine ("String literal to update: " + string<StringLiteralInfo> info)
            updateSnapshot fileLines info newValue
        | None -> failwithf "Could not find string literal after snapshot at line %d" snapshotLine
