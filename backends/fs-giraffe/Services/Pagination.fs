module Trendy.Services.Pagination

open System.Linq
open Trendy.Models

let page (size : int) (after : int) (collection : IQueryable<IPageable>) =
    query {
        for value in collection do
            where (value.Id > after)
            select value
            take size
    }
