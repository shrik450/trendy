module Trendy.Models.Link

open System.Linq
open Trendy.Utils
open Trendy.Contexts.LinksContext

type T = DatabaseTypes.Link

type AllowedParams = { Url: string; Notes: string }

let linkOfAllowedParams (userId: int) (allowedParams: AllowedParams) : T =
    { Id = 0
      Url = allowedParams.Url
      Notes = allowedParams.Notes
      UserId = userId }

let ofUser (collection: IQueryable<T>) (user: DatabaseTypes.User) =
    query {
        for link in collection do
            where (link.UserId = user.Id)
            select link
    }

let findById (collection: IQueryable<T>) (id: int) =
    query {
        for link in collection do
            where (link.Id = id)
            select link
            exactlyOneOrDefault
    }
    |> resultOfNullable "Not Found."
