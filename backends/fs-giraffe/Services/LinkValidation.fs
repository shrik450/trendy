module Trendy.Services.LinkValidation

open System.Text.RegularExpressions

open Trendy.Utils
open Trendy.Contexts.LinksContext
open Trendy.Models

/// Credit: @stephenhay's solution from https://mathiasbynens.be/demo/url-regex
let urlRegex = @"^https?://[^\s/$.?#].[^\s]*$"

let validateUrlPresence ({ Url = url }: Link.AllowedParams) =
    match url with
    | null
    | "" -> Error("Url", "Must be present")
    | _ -> Ok()

let validateUrlFormat ({ Url = url }: Link.AllowedParams) =
    let safeUrl = valueOrFallback url ""

    match Regex.Match(safeUrl, urlRegex).Success with
    | true -> Ok()
    | false -> Error("Url", "Must be a valid URL")

let validate (dbContext: LinksContext) (link: Link.AllowedParams) =
    let validations =
        [ validateUrlPresence
          validateUrlFormat ]

    let errors = Validation.validate validations link

    match Map.isEmpty errors with
    | true -> Ok link
    | false -> Error errors
