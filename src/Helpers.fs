module Fable.StaticPageGenerator.Helpers

open System.Collections.Generic
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Node.Exports
open Fable.Import.Node.Globals

let private templateCache = Dictionary<string, obj->string>()
let private handleBarsCompile (templateString: string): obj->string = import "compile" "handlebars"
let private marked (markdown: string): string = importDefault "marked"

/// Resolves a path to prevent using location of target JS file
/// Note the function is inline so `__dirname` will belong to the calling file
let inline resolve (p: string) =
    path.resolve(__dirname, p)

/// Parses a Handlebars template
let parseTemplate (path: string) (context: (string*obj) list) =
    let template =
        match templateCache.TryGetValue(path) with
        | true, template -> template
        | false, _ ->
            let template = fs.readFileSync(path).toString() |> handleBarsCompile
            templateCache.Add(path, template)
            template
    createObj context |> template

/// Parses a markdown file
let parseMarkdown (path: string) =
    fs.readFileSync(path).toString() |> marked

/// Parses a React element invoking ReactDOMServer.renderToString
let parseReact (el: React.ReactElement) =
    ReactDomServer.renderToString el

/// Parses a React element invoking ReactDOMServer.renderToStaticMarkup
let parseReactStatic (el: React.ReactElement) =
    ReactDomServer.renderToStaticMarkup el

let rec private ensureDirExists (dir: string) (cont: (unit->unit) option) =
    if fs.existsSync !^dir then
        match cont with Some c -> c() | None -> ()
    else
        ensureDirExists (path.dirname dir) (Some (fun () ->
            if not(fs.existsSync !^dir) then
                fs?mkdirSync(dir) |> ignore
            match cont with Some c -> c() | None -> ()
        ))

let writeFile (pth: string) (content: string) =
    ensureDirExists (path.dirname pth) None
    fs.writeFileSync(pth, content)

let readFile (path: string) =
    fs.readFileSync(path).toString()
