module Trendy.Services.LinkValidation

open Trendy.Contexts.LinksContext
open Trendy.Models

let validateUrlPresence ({ Url = url }: Link.AllowedParams) =
    match url with
    | null
    | "" -> Error("Url", "Must be present")
    | _ -> Ok()

let validate (dbContext: LinksContext) (link: Link.AllowedParams) =
    let validations = [ validateUrlPresence ]

    let errors = Validation.validate validations link

    match Map.isEmpty errors with
    | true -> Ok link
    | false -> Error errors
