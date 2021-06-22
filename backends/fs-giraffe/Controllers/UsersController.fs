namespace Trendy.Controllers

open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Giraffe

open Trendy
open Trendy.Contexts
open Trendy.Services

module UsersController =

    open LinksContext
    open Trendy.Models

    [<CLIMutable>]
    type RouteParams = { Id: int }

    [<CLIMutable>]
    type BodyParams =
        { Id: int
          Name: string
          Email: string
          Password: string
          ConfirmPassword: string }

    let userOfRouteParams (context: LinksContext) (requestParams: RouteParams) =
        requestParams.Id
        |> context.Users.FindAsync
        |> Utils.optionTaskOfNullableValueTask


    let userOfBodyParams (requestParams: BodyParams) : User.AllowedParams =
        { Id = -1
          Name = requestParams.Name
          Email = requestParams.Email
          Password = requestParams.Password
          ConfirmPassword = requestParams.ConfirmPassword }

    let userOfUpdateParams (id: int) (requestParams: BodyParams) : User.AllowedParams =
        { Id = id
          Name = requestParams.Name
          Email = requestParams.Email
          Password = requestParams.Password
          ConfirmPassword = requestParams.ConfirmPassword }

    let show (requestParams: RouteParams) : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let dbContext = ctx.GetService<LinksContext>()

                match! requestParams |> userOfRouteParams dbContext with
                | Some user -> return! Successful.ok (json user) next ctx
                | None -> return! RequestErrors.NOT_FOUND "Not Found." next ctx
            }

    let create : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let dbContext = ctx.GetService<LinksContext>()
                let! requestParams = ctx.BindJsonAsync<BodyParams>()

                let validationResult =
                    requestParams
                    |> userOfBodyParams
                    |> UserValidation.validate dbContext

                match validationResult with
                | Error errors -> return! RequestErrors.unprocessableEntity (json errors) next ctx
                | Ok validatedParams ->
                    validatedParams
                    |> User.userOfAllowedParams
                    |> dbContext.Users.Add
                    |> ignore

                    let! _ = dbContext.SaveChangesAsync()
                    return! Successful.CREATED "" next ctx
            }

    let update (requestParams: RouteParams) : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let dbContext = ctx.GetService<LinksContext>()
                let! updateParams = ctx.BindJsonAsync<BodyParams>()

                let validationResult =
                    updateParams
                    |> userOfUpdateParams requestParams.Id
                    |> UserValidation.validate dbContext

                match validationResult with
                | Error errors -> return! RequestErrors.unprocessableEntity (json errors) next ctx
                | Ok validatedParams ->
                    validatedParams
                    |> User.userOfAllowedParams
                    |> dbContext.Users.Update
                    |> ignore

                    let! _ = dbContext.SaveChangesAsync()
                    return! Successful.NO_CONTENT next ctx
            }


    let router : HttpHandler =
        choose [ GET >=> routeBind<RouteParams> "/{id}(/?)" show
                 POST >=> route "/" >=> create
                 PATCH
                 >=> routeBind<RouteParams> "/{id}(/?)" update ]
