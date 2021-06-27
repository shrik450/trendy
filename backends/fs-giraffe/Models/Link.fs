module Trendy.Models.Link

open Trendy.Contexts.LinksContext

type T = DatabaseTypes.Link

let ofUser (dbContext : LinksContext) (user : DatabaseTypes.User) =
    query {
        for link in dbContext.Links do
        where (link.User = user)
        select link
    }
