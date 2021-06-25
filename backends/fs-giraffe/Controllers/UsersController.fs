namespace Trendy.Controllers

open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Giraffe

open Trendy.Contexts
open Trendy.Services
open Trendy.Services.Authentication

module UsersController =

    open LinksContext
    open Trendy.Models

    [<CLIMutable>]
    type BodyParams =
        { Id: int
          Name: string
          Email: string
          Password: string
          ConfirmPassword: string }

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

    let show : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                match! currentUser ctx with
                | Ok user -> return! Successful.ok (json user) next ctx
                | Error _ -> return! RequestErrors.NOT_FOUND "Not Found." next ctx
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

    let update : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let dbContext = ctx.GetService<LinksContext>()
                let! updateParams = ctx.BindJsonAsync<BodyParams>()

                match! currentUser ctx with
                | Ok user ->
                    let validationResult =
                        updateParams
                        |> userOfUpdateParams user.Id
                        |> UserValidation.validate dbContext

                    match validationResult with
                    | Error errors ->
                        return!
                            RequestErrors.unprocessableEntity
                                (json errors)
                                next
                                ctx
                    | Ok validatedParams ->
                        validatedParams
                        |> User.userOfAllowedParams
                        |> dbContext.Users.Update
                        |> ignore

                        let! _ = dbContext.SaveChangesAsync()
                        return! Successful.NO_CONTENT next ctx
                | Error _ ->
                    return!
                        RequestErrors.notFound
                            (json {| error = "Not Found" |})
                            next
                            ctx
            }


    let router : HttpHandler =
        choose [ POST  >=> route "/" >=> create
                 GET   >=> route "/" >=> authorize >=> show
                 PATCH >=> route "/" >=> authorize >=> update ]
