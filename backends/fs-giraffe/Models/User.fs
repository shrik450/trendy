module Trendy.Models.User

open BCrypt.Net

open Trendy
open Trendy.Contexts.LinksContext
open FSharp.Control.Tasks

type T = DatabaseTypes.User

type AllowedParams =
    { Id: int
      Name: string
      Email: string
      Password: string
      ConfirmPassword: string }

let userOfAllowedParams (allowedParams : AllowedParams) : T =
    {
        Id = allowedParams.Id
        Name = allowedParams.Name
        Email = allowedParams.Email
        EncryptedPassword = allowedParams.Password |> BCrypt.EnhancedHashPassword
        Links = []
    }

let findByEmail (dbContext : LinksContext) (email : string) : Result<T, string> =
    query {
        for user in dbContext.Users do
        where (user.Email = email)
        select user
        exactlyOneOrDefault
    } |> Utils.resultOfNullable "User is not present"

let findByEmailAsync (dbContext : LinksContext) (email : string) =
    task {
        return findByEmail dbContext email
    }
