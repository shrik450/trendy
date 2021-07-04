module Trendy.Services.LinkValidation

open Trendy.Contexts.LinksContext
open Trendy.Models

val validate :
    LinksContext ->
    Link.AllowedParams ->
    Result<Link.AllowedParams, Validation.ValidationErrors>
