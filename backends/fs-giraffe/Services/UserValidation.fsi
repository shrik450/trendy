namespace Trendy.Services

open Trendy.Contexts.LinksContext
open Trendy.Models

module UserValidation =
    val validate :
        LinksContext ->
        User.AllowedParams ->
        Result<User.AllowedParams, Validation.ValidationErrors>
