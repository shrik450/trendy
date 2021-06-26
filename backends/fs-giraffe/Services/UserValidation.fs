module Trendy.Services.UserValidation

open Trendy.Utils
open Trendy.Models
open Trendy.Contexts.LinksContext

let validateNamePresence ({ Name = name }: User.AllowedParams) =
    match name with
    | null | "" -> Error("Name", "must be present.")
    | _ -> Ok()

let validateEmailPresence ({ Email = email }: User.AllowedParams) =
    match email with
    | null | "" -> Error("Email", "must be present.")
    | _ -> Ok()

let validateEmailUniqueness
    (dbContext: LinksContext)
    ({ Id = id; Email = email }: User.AllowedParams)
    =
    query {
        for user in dbContext.Users do
            where (user.Id <> id && user.Email = email)
            count
    }
    |> function
    | 0 -> Ok()
    | _ -> Error("Email", "has already been taken.")

let validatePasswordConfirmation
    ({ Password = password
       ConfirmPassword = confirmPassword }: User.AllowedParams)
    =
    match password = confirmPassword with
    | true -> Ok()
    | false -> Error("Password", "does not match confirmation.")

let validatePasswordLength ({ Password = password }: User.AllowedParams) =
    let safePassword = valueOrFallback password ""
    match String.length safePassword >= 8 with
    | true -> Ok()
    | false -> Error("Password", "must be 8 characters or longer.")

let validate (dbContext: LinksContext) (user: User.AllowedParams) =
    let validations =
        [ validateNamePresence
          validateEmailPresence
          validateEmailUniqueness dbContext
          validatePasswordConfirmation
          validatePasswordLength ]

    let errors = Validation.validate validations user

    match Map.isEmpty errors with
    | true -> Ok user
    | false -> Error errors
