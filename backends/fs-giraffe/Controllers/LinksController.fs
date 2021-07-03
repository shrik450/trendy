module Trendy.Controllers.LinksController

open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Giraffe

open Microsoft.Extensions.Logging
open Trendy.Services
open Trendy.Utils
open Trendy.Contexts.LinksContext
open Trendy.Models
open Trendy.Services.Authentication

type RouteParams = { Id: string }

[<CLIMutable>]
type QueryParams = { After: int; Size: int }

type SerializedLink = { Id: int; Url: string; Notes: string }

let pageOfQueryParams { After = after; Size = size } = Pagination.page after size

let queryParamsOrDefault (a: QueryParams) =
    { After = valueOrFallback a.After 0
      Size = valueOrFallback a.Size 25 }

let serializeLink (link: Link.T) =
    { Id = link.Id
      Url = link.Url
      Notes = link.Notes }

let index : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let dbContext = ctx.GetService<LinksContext>()
            let log = ctx.GetLogger<ILogger>()

            match! currentUser ctx with
            | Ok user ->
                log.LogInformation("Processing for user {u}", user)

                let records =
                    user
                    |> Link.ofUser dbContext
                    |> (ctx.BindQueryString<QueryParams>()
                        |> queryParamsOrDefault
                        |> pageOfQueryParams)
                    |> Seq.toList
                    |> List.map serializeLink

                return! Successful.ok (json {| links = records |}) next ctx
            | Error errorMessage ->
                log.LogCritical(
                    "Error when finding user in LinksController#index: #{e}",
                    errorMessage
                )

                return! Successful.NO_CONTENT next ctx
        }

let show (requestParams: RouteParams) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) -> text "Hello!" next ctx

let router : HttpHandler =
    authorize
    >=> choose [ GET >=> route "/" >=> index
                 GET >=> routeBind<RouteParams> "/{id}(/?)" show ]
