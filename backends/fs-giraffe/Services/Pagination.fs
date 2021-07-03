module Trendy.Services.Pagination

open System.Linq
open System.Threading.Tasks
open FSharp.Control.Tasks
open Trendy.Models


let page
    (size: int)
    (after: int)
    (collection: IQueryable<#IPageable>)
    : IQueryable<#IPageable>
    =
        query {
            for value in collection.DefaultIfEmpty() do
                where (value.Id > after)
                select value
                take size
        }

let asyncPage (size: int) (after: int) (collection: Task<IQueryable<#IPageable>>) =
    task {
        let! records = collection
        return page size after records
    }
