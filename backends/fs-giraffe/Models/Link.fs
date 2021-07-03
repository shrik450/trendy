module Trendy.Models.Link

open Trendy.Utils
open Trendy.Contexts.LinksContext

type T = DatabaseTypes.Link

let ofUser (dbContext : LinksContext) (user : DatabaseTypes.User) =
    query {
        for link in dbContext.Links do
        where (link.UserId = user.Id)
        select link
    }
