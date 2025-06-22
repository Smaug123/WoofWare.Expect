module internal WoofWare.Expect.Diff

open System.Collections.Generic

/// Position in a sequence
type Position = | Position of int

/// A diff operation
type DiffOperation =
    | Match of posA : Position * posB : Position * line : string
    | Delete of posA : Position * line : string
    | Insert of posB : Position * line : string

/// A match between positions in two sequences
type LineMatch =
    {
        PosA : Position
        PosB : Position
        Line : string
    }

/// Result of finding unique lines in a sequence
type UniqueLines =
    {
        /// Map from line content to its position (only for unique lines)
        LinePositions : Map<string, Position>
        /// All line counts (for verification)
        LineCounts : Map<string, int>
    }

/// Find lines that appear exactly once in a sequence
let findUniqueLines (lines : string array) : UniqueLines =
    let positions = Dictionary<string, Position> ()
    let counts = Dictionary<string, int> ()

    lines
    |> Array.iteri (fun i line ->
        if counts.ContainsKey (line) then
            counts.[line] <- counts.[line] + 1
        else
            counts.[line] <- 1
            positions.[line] <- Position i
    )

    let uniquePositions =
        positions
        |> Seq.filter (fun kvp -> counts.[kvp.Key] = 1)
        |> Seq.map (fun kvp -> (kvp.Key, kvp.Value))
        |> Map.ofSeq

    let allCounts = counts |> Seq.map (fun kvp -> (kvp.Key, kvp.Value)) |> Map.ofSeq

    {
        LinePositions = uniquePositions
        LineCounts = allCounts
    }

/// Find longest increasing subsequence based on B positions
let longestIncreasingSubsequence (matches : LineMatch array) : LineMatch list =
    let n = matches.Length

    if n = 0 then
        []
    else
        // Dynamic programming arrays
        let lengths = Array.create n 1
        let parents = Array.create n -1

        // Build LIS
        for i in 1 .. n - 1 do
            for j in 0 .. i - 1 do
                let (Position bj) = matches.[j].PosB
                let (Position bi) = matches.[i].PosB

                if bj < bi && lengths.[j] + 1 > lengths.[i] then
                    lengths.[i] <- lengths.[j] + 1
                    parents.[i] <- j

        // Find longest sequence
        let maxLength = Array.max lengths
        let endIndex = Array.findIndex ((=) maxLength) lengths

        // Reconstruct sequence
        let rec reconstruct idx acc =
            if idx = -1 then
                acc
            else
                reconstruct parents.[idx] (matches.[idx] :: acc)

        reconstruct endIndex []

/// Simple Myers diff implementation as fallback
let myersDiff (a : string array) (b : string array) : DiffOperation list =
    let rec diffHelper (i : int) (j : int) (acc : DiffOperation list) =
        match i < a.Length, j < b.Length with
        | false, false -> List.rev acc
        | true, false ->
            let deletes =
                [ i .. a.Length - 1 ] |> List.map (fun idx -> Delete (Position idx, a.[idx]))

            (List.rev acc) @ deletes
        | false, true ->
            let inserts =
                [ j .. b.Length - 1 ] |> List.map (fun idx -> Insert (Position idx, b.[idx]))

            (List.rev acc) @ inserts
        | true, true ->
            if a.[i] = b.[j] then
                diffHelper (i + 1) (j + 1) (Match (Position i, Position j, a.[i]) :: acc)
            else
                // Look ahead for matches (simple heuristic)
                let lookAhead = 3

                let aheadMatch =
                    [ 1 .. min lookAhead (min (a.Length - i) (b.Length - j)) ]
                    |> List.tryFind (fun k -> a.[i + k - 1] = b.[j + k - 1])

                match aheadMatch with
                | Some k when k <= 2 ->
                    // Delete/insert to get to the match
                    let ops =
                        [ 0 .. k - 2 ]
                        |> List.collect (fun offset ->
                            [
                                Delete (Position (i + offset), a.[i + offset])
                                Insert (Position (j + offset), b.[j + offset])
                            ]
                        )

                    diffHelper (i + k - 1) (j + k - 1) (List.rev ops @ acc)
                | _ ->
                    // No close match, just delete and insert
                    diffHelper (i + 1) j (Delete (Position i, a.[i]) :: acc)

    diffHelper 0 0 []

/// Main patience diff algorithm
let rec patience (a : string array) (b : string array) : DiffOperation list =
    // Handle empty sequences
    match a.Length, b.Length with
    | 0, 0 -> []
    | 0, _ -> b |> Array.mapi (fun i line -> Insert (Position i, line)) |> Array.toList
    | _, 0 -> a |> Array.mapi (fun i line -> Delete (Position i, line)) |> Array.toList
    | _, _ ->
        // Find unique lines
        let uniqueA = findUniqueLines a
        let uniqueB = findUniqueLines b

        // Find common unique lines
        let commonUniques =
            Set.intersect
                (uniqueA.LinePositions |> Map.toSeq |> Seq.map fst |> Set.ofSeq)
                (uniqueB.LinePositions |> Map.toSeq |> Seq.map fst |> Set.ofSeq)

        if Set.isEmpty commonUniques then
            // No unique common lines, fall back to Myers
            myersDiff a b
        else
            // Build matches for unique common lines
            let matches =
                commonUniques
                |> Set.toArray
                |> Array.map (fun line ->
                    {
                        PosA = uniqueA.LinePositions.[line]
                        PosB = uniqueB.LinePositions.[line]
                        Line = line
                    }
                )
                |> Array.sortBy (fun m -> m.PosA)

            // Find LIS
            let anchorMatches = longestIncreasingSubsequence matches |> List.toArray

            // Build diff imperatively
            let result = ResizeArray<DiffOperation> ()
            let mutable prevA = 0
            let mutable prevB = 0

            // Process each anchor
            for anchor in anchorMatches do
                let (Position anchorA) = anchor.PosA
                let (Position anchorB) = anchor.PosB

                // Add diff for section before this anchor
                if prevA < anchorA || prevB < anchorB then
                    let sectionA = a.[prevA .. anchorA - 1]
                    let sectionB = b.[prevB .. anchorB - 1]
                    let sectionDiff = patience sectionA sectionB

                    // Adjust positions and add to result
                    for op in sectionDiff do
                        match op with
                        | Match (Position pa, Position pb, line) ->
                            result.Add (Match (Position (pa + prevA), Position (pb + prevB), line))
                        | Delete (Position pa, line) -> result.Add (Delete (Position (pa + prevA), line))
                        | Insert (Position pb, line) -> result.Add (Insert (Position (pb + prevB), line))

                // Add the anchor match
                result.Add (Match (anchor.PosA, anchor.PosB, anchor.Line))

                // Update positions
                prevA <- anchorA + 1
                prevB <- anchorB + 1

            // Handle remaining elements after last anchor
            if prevA < a.Length || prevB < b.Length then
                let remainingA = a.[prevA..]
                let remainingB = b.[prevB..]
                let remainingDiff = patience remainingA remainingB

                for op in remainingDiff do
                    match op with
                    | Match (Position pa, Position pb, line) ->
                        result.Add (Match (Position (pa + prevA), Position (pb + prevB), line))
                    | Delete (Position pa, line) -> result.Add (Delete (Position (pa + prevA), line))
                    | Insert (Position pb, line) -> result.Add (Insert (Position (pb + prevB), line))

            result |> Seq.toList

/// Format diff operations for display
let formatWithLineNumbers (ops : DiffOperation list) : string list =
    ops
    |> List.map (fun op ->
        match op with
        | Match (Position a, Position b, line) -> sprintf "  %3d %3d  %s" a b line
        | Delete (Position a, line) -> sprintf "- %3d      %s" a line
        | Insert (Position b, line) -> sprintf "+     %3d  %s" b line
    )

let format (ops : DiffOperation list) : string list =
    ops
    |> List.map (fun op ->
        match op with
        | Match (Position _, Position _, line) -> sprintf "  %s" line
        | Delete (Position _, line) -> sprintf "- %s" line
        | Insert (Position _, line) -> sprintf "+ %s" line
    )

/// Compute diff statistics
type DiffStats =
    {
        Matches : int
        Deletions : int
        Insertions : int
        TotalOperations : int
    }

let computeStats (ops : DiffOperation list) : DiffStats =
    let counts =
        ops
        |> List.fold
            (fun (m, d, i) op ->
                match op with
                | Match _ -> (m + 1, d, i)
                | Delete _ -> (m, d + 1, i)
                | Insert _ -> (m, d, i + 1)
            )
            (0, 0, 0)

    let matches, deletions, insertions = counts

    {
        Matches = matches
        Deletions = deletions
        Insertions = insertions
        TotalOperations = matches + deletions + insertions
    }
