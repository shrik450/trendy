module Trendy.Controllers.LinksController

open System
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Giraffe

open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Logging
open Microsoft.VisualBasic.CompilerServices
open Trendy.Services
open Trendy.Utils
open Trendy.Contexts.LinksContext
open Trendy.Models
open Trendy.Services.Authentication
open Trendy.Controllers.Common

[<CLIMutable>]
type RouteParams = { Id: int }

[<CLIMutable>]
type QueryParams =
    // int is a primitive, so an uninitialized int is 0.
    // Because we can't distinguish from an uninitialized 0 and a 0 sent
    // by the user, we have to specifically mark these ints as nullable.
    { After: int Nullable
      Size: int Nullable }

type StrongQueryParams = { After: int; Size: int }

[<CLIMutable>]
type BodyParams = Link.AllowedParams

type SerializedLink = { Id: int; Url: string; Notes: string }

let pageOfQueryParams ({ After = after; Size = size }: StrongQueryParams) =
    Pagination.page size after

let queryParamsOrDefault (a: QueryParams) : StrongQueryParams =
    { After = valueOrFallbackN a.After 0
      Size = valueOrFallbackN a.Size 25 }

let updateParamsOrDefault (a: Link.T) (b: BodyParams) : Link.T =
    { Id = a.Id
      Url = a.Url
      Notes = valueOrFallback b.Notes a.Notes
      UserId = a.UserId }

let serializeLink (link: Link.T) =
    { Id = link.Id
      Url = link.Url
      Notes = link.Notes }

let index : HttpHandler =
    fun next ctx ->
        task {
            let dbContext = ctx.GetService<LinksContext>()
            let log = ctx.GetLogger<ILogger>()

            match! currentUser ctx with
            | Ok user ->
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
                | Error _ -> return! handleNotFound next ctx
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

let update (requestParams: RouteParams) : HttpHandler =
    fun next ctx ->
        task {
            let dbContext = ctx.GetService<LinksContext>()
            let log = ctx.GetLogger<ILogger>()

            match! currentUser ctx with
            | Ok user ->
                let userLinks = Link.ofUser dbContext.Links user

                match Link.findById userLinks requestParams.Id with
                | Ok link ->
                    let! updateParams = ctx.BindJsonAsync<BodyParams>()

                    match LinkValidation.validate dbContext updateParams with
                    | Ok updateParams ->
                        dbContext.Entry(link).State <- EntityState.Detached

                        updateParams
                        |> updateParamsOrDefault link
                        |> dbContext.Links.Update
                        |> ignore

                        let! _ = dbContext.SaveChangesAsync()
                        return! Successful.NO_CONTENT next ctx
                    | Error errors ->
                        return! handleInvalidEntity errors next ctx
                | Error _ -> return! handleNotFound next ctx
            | Error errorMessage ->
                return!
                    handleMissingUser log "links update" errorMessage next ctx

        }

let router : HttpHandler =
    authorize
    >=> choose [ GET >=> route "/" >=> index
                 GET >=> routeBind<RouteParams> "/{id}(/?)" show
                 POST >=> route "/" >=> create
                 PATCH
                 >=> routeBind<RouteParams> "/{id}(/?)" update ]
