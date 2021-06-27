module Trendy.Controllers.LinksController

open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Giraffe

open Trendy.Models
open Trendy.Services
open Trendy.Utils
open Trendy.Contexts.LinksContext
open Trendy.Services.Authentication

type RouteParams = { Id: string }

type QueryParams =
    { After : int
      Size : int }

let pageOfQueryParams {After = after; Size = size} =
    Pagination.page after size

let index : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let dbContext = ctx.GetService<LinksContext>()
            match ctx.TryBindQueryString<QueryParams> () with
            | Ok q ->
                let page = q |> pageOfQueryParams
            | Error _ ->
                return! text "Hello" next ctx
        }

let show (requestParams : RouteParams) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        text "Hello!" next ctx

let router : HttpHandler =
    authorize
    >=> choose [ GET >=> route "/" >=> index
                 GET >=> routeBind<RouteParams> "/{id}(/?)" show ]
