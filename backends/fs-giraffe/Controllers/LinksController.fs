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
open Trendy.Controllers.Common

[<CLIMutable>]
type RouteParams = { Id: int }

[<CLIMutable>]
type QueryParams = { After: int; Size: int }

[<CLIMutable>]
type BodyParams = Link.AllowedParams

type SerializedLink = { Id: int; Url: string; Notes: string }

let pageOfQueryParams { After = after; Size = size } =
    Pagination.page after size

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
                    |> Link.ofUser dbContext.Links
                    |> (ctx.BindQueryString<QueryParams>()
                        |> queryParamsOrDefault
                        |> pageOfQueryParams)
                    |> Seq.toList
                    |> List.map serializeLink

                return! Successful.ok (json {| links = records |}) next ctx
            | Error errorMessage ->
                return!
                    handleMissingUser log "links index" errorMessage next ctx
        }

let show (requestParams: RouteParams) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let dbContext = ctx.GetService<LinksContext>()
            let log = ctx.GetLogger<ILogger>()

            match! currentUser ctx with
            | Ok user ->
                let userLinks = Link.ofUser dbContext.Links user

                match Link.findById userLinks requestParams.Id with
                | Ok link ->
                    return! Successful.ok (json (serializeLink link)) next ctx
                | Error _ ->
                    return! RequestErrors.notFound notFoundResponse next ctx
            | Error errorMessage ->
                return!
                    handleMissingUser log "links index" errorMessage next ctx
        }

let create : HttpHandler =
    fun next ctx ->
        task {
            let dbContext = ctx.GetService<LinksContext>()
            let log = ctx.GetLogger<ILogger>()

            match! currentUser ctx with
            | Ok user ->
                let! linkParams = ctx.BindJsonAsync<BodyParams>()

                match LinkValidation.validate dbContext linkParams with
                | Error errors -> return! handleInvalidEntity errors next ctx
                | Ok validatedParams ->
                    let link =
                        Link.linkOfAllowedParams user.Id validatedParams

                    dbContext.Links.Add(link) |> ignore

                    let! _ = dbContext.SaveChangesAsync()

                    return!
                        Successful.CREATED
                            {| link = serializeLink link |}
                            next
                            ctx
            | Error errorMessage ->
                return!
                    handleMissingUser log "links create" errorMessage next ctx
        }

let router : HttpHandler =
    authorize
    >=> choose [ GET >=> route "/" >=> index
                 GET >=> routeBind<RouteParams> "/{id}(/?)" show
                 POST >=> route "/" >=> create ]
