namespace WoofWare.Expect

open System.Collections.Generic

/// A unit of measure tagging "positions in a sequence".
[<Measure>]
type pos

/// Position in a sequence
type Position = int<pos>

/// A Patience diff is composed of a sequence of transformations to get from one string to another.
/// This represents those transformations.
type DiffOperation =
    /// This line is the same on both sides of the diff.
    /// On the left, it appears at position posA. On the right, at position posB.
    | Match of posA : Position * posB : Position * line : string
    /// Delete this line, which is at this position.
    | Delete of posA : Position * line : string
    /// Insert this line at the given position.
    | Insert of posB : Position * line : string

/// The diff between two line-oriented pieces of text.
type Diff = private | Diff of DiffOperation list

/// A match between positions in two sequences
type internal LineMatch =
    {
        PosA : Position
        PosB : Position
        Line : string
    }

/// Result of finding unique lines in a sequence
type internal UniqueLines =
    {
        /// Map from line content to its position (only for unique lines)
        LinePositions : Map<string, Position>
        /// All line counts (for verification)
        LineCounts : Map<string, int>
    }

[<RequireQualifiedAccess>]
module Diff =
    /// Find lines that appear exactly once in a sequence
    let private findUniqueLines (lines : string array) : UniqueLines =
        let positions = Dictionary<string, Position> ()
        let counts = Dictionary<string, int> ()

        lines
        |> Array.iteri (fun i line ->
            if counts.ContainsKey (line) then
                counts.[line] <- counts.[line] + 1
            else
                counts.[line] <- 1
                positions.[line] <- i * 1<pos>
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
    let private longestIncreasingSubsequence (matches : LineMatch array) : LineMatch list =
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
                    let bj = matches.[j].PosB
                    let bi = matches.[i].PosB

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

    /// Simple Myers diff implementation. You probably want to use `patience` instead, for more human-readable diffs.
    let myers (a : string array) (b : string array) : Diff =
        let rec diffHelper (i : int) (j : int) (acc : DiffOperation list) =
            match i < a.Length, j < b.Length with
            | false, false -> List.rev acc
            | true, false ->
                let deletes =
                    [ i .. a.Length - 1 ] |> List.map (fun idx -> Delete (idx * 1<pos>, a.[idx]))

                (List.rev acc) @ deletes
            | false, true ->
                let inserts =
                    [ j .. b.Length - 1 ] |> List.map (fun idx -> Insert (idx * 1<pos>, b.[idx]))

                (List.rev acc) @ inserts
            | true, true ->
                if a.[i] = b.[j] then
                    diffHelper (i + 1) (j + 1) (Match (i * 1<pos>, j * 1<pos>, a.[i]) :: acc)
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
                                    Delete ((i + offset) * 1<pos>, a.[i + offset])
                                    Insert ((j + offset) * 1<pos>, b.[j + offset])
                                ]
                            )

                        diffHelper (i + k - 1) (j + k - 1) (List.rev ops @ acc)
                    | _ ->
                        // No close match, just delete and insert
                        diffHelper (i + 1) j (Delete (i * 1<pos>, a.[i]) :: acc)

        diffHelper 0 0 [] |> Diff

    /// Patience diff: a human-readable line-based diff.
    /// Operates on lines of string; the function `patience` will split on lines for you.
    let rec patienceLines (a : string array) (b : string array) : Diff =
        // Handle empty sequences
        match a.Length, b.Length with
        | 0, 0 -> [] |> Diff
        | 0, _ ->
            b
            |> Array.mapi (fun i line -> Insert (i * 1<pos>, line))
            |> Array.toList
            |> Diff
        | _, 0 ->
            a
            |> Array.mapi (fun i line -> Delete (i * 1<pos>, line))
            |> Array.toList
            |> Diff
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
                myers a b
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
                let mutable prevA = 0<pos>
                let mutable prevB = 0<pos>

                // Process each anchor
                for anchor in anchorMatches do
                    let anchorA = anchor.PosA
                    let anchorB = anchor.PosB

                    // Add diff for section before this anchor
                    if prevA < anchorA || prevB < anchorB then
                        let sectionA = a.[prevA / 1<pos> .. anchorA / 1<pos> - 1]
                        let sectionB = b.[prevB / 1<pos> .. anchorB / 1<pos> - 1]
                        let (Diff sectionDiff) = patienceLines sectionA sectionB

                        // Adjust positions and add to result
                        for op in sectionDiff do
                            match op with
                            | Match (pa, pb, line) -> result.Add (Match ((pa + prevA), (pb + prevB), line))
                            | Delete (pa, line) -> result.Add (Delete ((pa + prevA), line))
                            | Insert (pb, line) -> result.Add (Insert ((pb + prevB), line))

                    // Add the anchor match
                    result.Add (Match (anchor.PosA, anchor.PosB, anchor.Line))

                    // Update positions
                    prevA <- anchorA + 1<pos>
                    prevB <- anchorB + 1<pos>

                // Handle remaining elements after last anchor
                if prevA / 1<pos> < a.Length || prevB / 1<pos> < b.Length then
                    let remainingA = a.[prevA / 1<pos> ..]
                    let remainingB = b.[prevB / 1<pos> ..]
                    let (Diff remainingDiff) = patienceLines remainingA remainingB

                    for op in remainingDiff do
                        match op with
                        | Match (pa, pb, line) -> result.Add (Match ((pa + prevA), (pb + prevB), line))
                        | Delete (pa, line) -> result.Add (Delete ((pa + prevA), line))
                        | Insert (pb, line) -> result.Add (Insert ((pb + prevB), line))

                result |> Seq.toList |> Diff

    /// Patience diff: a human-readable line-based diff.
    let patience (a : string) (b : string) =
        patienceLines (a.Split '\n') (b.Split '\n')

    /// Format the diff as a human-readable string, including line numbers at the left.
    let formatWithLineNumbers (Diff ops) : string =
        ops
        |> List.map (fun op ->
            match op with
            | Match (a, b, line) -> sprintf "  %3d %3d  %s" a b line
            | Delete (a, line) -> sprintf "- %3d      %s" a line
            | Insert (b, line) -> sprintf "+     %3d  %s" b line
        )
        |> String.concat "\n"

    /// Format the diff as a human-readable string.
    let format (Diff ops) : string =
        ops
        |> List.map (fun op ->
            match op with
            | Match (_, _, line) -> sprintf "  %s" line
            | Delete (_, line) -> sprintf "- %s" line
            | Insert (_, line) -> sprintf "+ %s" line
        )
        |> String.concat "\n"

    /// Compute diff statistics
    type internal DiffStats =
        {
            Matches : int
            Deletions : int
            Insertions : int
            TotalOperations : int
        }

    let internal computeStats (ops : DiffOperation list) : DiffStats =
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
