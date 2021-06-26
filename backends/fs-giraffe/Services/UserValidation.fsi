module Trendy.Services.UserValidation

open Trendy.Contexts.LinksContext
open Trendy.Models

val validate :
    LinksContext ->
    User.AllowedParams ->
    Result<User.AllowedParams, Validation.ValidationErrors>
