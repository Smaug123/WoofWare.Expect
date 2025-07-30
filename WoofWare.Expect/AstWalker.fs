namespace WoofWare.Expect

open Fantomas.FCS.Diagnostics
open Fantomas.FCS.Syntax
open Fantomas.FCS.Text

type internal SnapshotLocation =
    {
        KeywordRange : Range
        Keyword : string
        ReplacementRange : Range
    }

[<RequireQualifiedAccess>]
module internal AstWalker =

    let private snapshotSignifier =
        [ "snapshot" ; "snapshotJson" ; "snapshotList" ; "snapshotThrows" ]
        |> Set.ofList

    /// Check if this is a call to snapshotList (or any other snapshot method we care about)
    /// Returns the identifier that is the snapshot invocation, and its range.
    let private isSnapshotCall (funcExpr : SynExpr) : (string * Range) option =
        match funcExpr with
        | SynExpr.Ident (ident) when snapshotSignifier.Contains ident.idText -> Some (ident.idText, ident.idRange)
        | SynExpr.LongIdent (_, longIdent, _, _) ->
            match longIdent.IdentsWithTrivia with
            | [] -> None
            | ids ->
                match List.last ids with
                | SynIdent.SynIdent (ident, _) ->
                    if snapshotSignifier.Contains ident.idText then
                        Some (ident.idText, ident.idRange)
                    else
                        None
        | _ -> None

    /// Extract the argument from a method application
    let private getMethodArgument (expr : SynExpr) =
        match expr with
        | SynExpr.App (_, _, _, argExpr, _) -> Some argExpr
        | _ -> None

    /// Walk expressions looking for our target
    let rec findSnapshotListCalls (targetLine : int) (methodName : string) (expr : SynExpr) : SnapshotLocation list =
        match expr with
        // Direct method application
        | SynExpr.App (_, _, funcExpr, argExpr, range) ->
            match isSnapshotCall funcExpr with
            | Some (keyword, keywordRange) ->
                if range.StartLine <= targetLine && targetLine <= range.EndLine then
                    match argExpr with
                    | SynExpr.ArrayOrList (isList, _, argRange) when isList ->
                        // It's a list literal
                        [
                            {
                                ReplacementRange = argRange
                                KeywordRange = keywordRange
                                Keyword = keyword
                            }
                        ] // Text will be extracted separately
                    | SynExpr.ArrayOrListComputed (isArray, innerExpr, argRange) when not isArray ->
                        // It's a list comprehension
                        [
                            {
                                ReplacementRange = argRange
                                KeywordRange = keywordRange
                                Keyword = keyword
                            }
                        ]
                    | _ ->
                        // It could be a variable reference or other expression
                        [
                            {
                                ReplacementRange = argExpr.Range
                                KeywordRange = keywordRange
                                Keyword = keyword
                            }
                        ]
                else
                    []
            | None ->
                // Other app variations
                findSnapshotListCalls targetLine methodName funcExpr
                @ findSnapshotListCalls targetLine methodName argExpr

        // Nested in paren
        | SynExpr.Paren (innerExpr, _, _, _) -> findSnapshotListCalls targetLine methodName innerExpr

        // Sequential expressions (e.g., in a do block)
        | SynExpr.Sequential (_, _, expr1, expr2, _, _) ->
            findSnapshotListCalls targetLine methodName expr1
            @ findSnapshotListCalls targetLine methodName expr2

        // Let bindings
        | SynExpr.LetOrUse (_, _, bindings, bodyExpr, _, _) ->
            let bindingResults =
                bindings
                |> List.collect (fun binding ->
                    match binding with
                    | SynBinding (expr = expr) -> findSnapshotListCalls targetLine methodName expr
                )

            bindingResults @ findSnapshotListCalls targetLine methodName bodyExpr

        // Match expressions
        | SynExpr.Match (_, _, clauses, _, _) ->
            clauses
            |> List.collect (fun (SynMatchClause (resultExpr = expr)) ->
                findSnapshotListCalls targetLine methodName expr
            )

        // If/then/else
        | SynExpr.IfThenElse (_, thenExpr, elseExprOpt, _, _, _, _) ->
            let thenResults = findSnapshotListCalls targetLine methodName thenExpr

            let elseResults =
                match elseExprOpt with
                | Some elseExpr -> findSnapshotListCalls targetLine methodName elseExpr
                | None -> []

            thenResults @ elseResults

        // Lambda
        | SynExpr.Lambda (body = bodyExpr) -> findSnapshotListCalls targetLine methodName bodyExpr

        // Computation expression
        | SynExpr.ComputationExpr (_, innerExpr, _) -> findSnapshotListCalls targetLine methodName innerExpr

        // Default case - no results
        | _ -> []

    /// Walk a module or namespace looking for expressions
    let rec findInModuleDecls (targetLine : int) (methodName : string) (decls : SynModuleDecl list) =
        decls
        |> List.collect (fun decl ->
            match decl with
            | SynModuleDecl.Let (_, bindings, _) ->
                bindings
                |> List.collect (fun binding ->
                    match binding with
                    | SynBinding (expr = expr) -> findSnapshotListCalls targetLine methodName expr
                )

            | SynModuleDecl.Expr (expr, _) -> findSnapshotListCalls targetLine methodName expr

            | SynModuleDecl.NestedModule (decls = nestedDecls) -> findInModuleDecls targetLine methodName nestedDecls

            | SynModuleDecl.Types (typeDefs, _) ->
                typeDefs
                |> List.collect (fun typeDef ->
                    match typeDef with
                    | SynTypeDefn (typeRepr = SynTypeDefnRepr.ObjectModel (members = members)) ->
                        members
                        |> List.collect (fun member' ->
                            match member' with
                            | SynMemberDefn.Member (memberBinding, _) ->
                                match memberBinding with
                                | SynBinding (expr = expr) -> findSnapshotListCalls targetLine methodName expr
                            | _ -> []
                        )
                    | _ -> []
                )

            | SynModuleDecl.HashDirective _
            | SynModuleDecl.Attributes _
            | SynModuleDecl.ModuleAbbrev _
            | SynModuleDecl.Exception _
            | SynModuleDecl.Open _ -> []
            | SynModuleDecl.NamespaceFragment (SynModuleOrNamespace (decls = decls)) ->
                findInModuleDecls targetLine methodName decls
        )

    /// Extract the exact text from source given a range
    let extractTextFromSource (sourceText : string) (range : Range) : string =
        let lines = sourceText.Split ('\n')

        if range.StartLine = range.EndLine then
            // Single line
            let line = lines.[range.StartLine - 1]
            line.Substring (range.StartColumn, range.EndColumn - range.StartColumn)
        else
            // Multi-line
            let firstLine = lines.[range.StartLine - 1].Substring (range.StartColumn)

            let middleLines =
                if range.EndLine - range.StartLine > 1 then
                    lines.[range.StartLine .. range.EndLine - 2] |> String.concat "\n"
                else
                    ""

            let lastLine = lines.[range.EndLine - 1].Substring (0, range.EndColumn)

            [ firstLine ; middleLines ; lastLine ]
            |> List.filter (fun s -> s <> "")
            |> String.concat "\n"

    /// Main function to find snapshot list locations
    let findSnapshotList
        (infoFilePath : string)
        (lines : string[])
        (lineNumber : int)
        (methodName : string) // e.g., "snapshotList"
        : SnapshotLocation
        =
        let sourceText = SourceText.ofString (String.concat "\n" lines)

        // Parse the file
        let parsedInput, diagnostics = Fantomas.FCS.Parse.parseFile false sourceText []

        // Check for parse errors
        if
            diagnostics
            |> List.exists (fun d -> d.Severity = FSharpDiagnosticSeverity.Error)
        then
            failwithf "Parse errors in file %s: %A" infoFilePath diagnostics

        // Walk the AST
        let results =
            match parsedInput with
            | ParsedInput.ImplFile (ParsedImplFileInput (contents = modules)) ->
                modules
                |> List.collect (fun moduleOrNs ->
                    match moduleOrNs with
                    | SynModuleOrNamespace (decls = decls) -> findInModuleDecls lineNumber methodName decls
                )
            | ParsedInput.SigFile _ -> failwith "unexpected: signature files can't contain expressions"

        // Find the closest match
        results
        |> Seq.filter (fun loc ->
            loc.KeywordRange.StartLine <= lineNumber
            && lineNumber <= loc.KeywordRange.EndLine
        )
        |> Seq.exactlyOne
