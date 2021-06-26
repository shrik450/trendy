module Trendy.Controllers.UsersController


open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks
open Giraffe

open Trendy.Utils
open Trendy.Models
open Trendy.Contexts.LinksContext
open Trendy.Services
open Trendy.Services.Authentication

[<CLIMutable>]
type BodyParams =
    { Id: int
      Name: string
      Email: string
      Password: string
      ConfirmPassword: string }

type SerializableUser = { Name: string; Email: string }

let userOfBodyParams (requestParams: BodyParams) : User.AllowedParams =
    { Id = 0
      Name = requestParams.Name
      Email = requestParams.Email
      Password = requestParams.Password
      ConfirmPassword = requestParams.ConfirmPassword }

let userOfUpdateParams (user : User.T) (requestParams: BodyParams) : User.AllowedParams =
    { Id = user.Id
      Name = valueOrFallback requestParams.Name user.Name
      Email = valueOrFallback requestParams.Email user.Email
      Password = requestParams.Password
      ConfirmPassword = requestParams.ConfirmPassword }

let serializableUserOfUser (user: User.T) =
    { Name = user.Name; Email = user.Email }

let show : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let logger = ctx.GetLogger<ILogger>()

            match! currentUser ctx with
            | Ok user ->
                let response = user |> serializableUserOfUser |> json
                return! Successful.ok response next ctx
            | Error errorMessage ->
                logger.LogInformation(
                    "Could not find user from authenticated token: {ErrorMessage}",
                    errorMessage
                )

                return! RequestErrors.NOT_FOUND "Not Found." next ctx
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
            | Error errors ->
                let response = {| errors = errors |} |> json
                return! RequestErrors.unprocessableEntity response next ctx
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
                    |> userOfUpdateParams user
                    |> UserValidation.validate dbContext

                match validationResult with
                | Error errors ->
                    let response = {| errors = errors |} |> json
                    return! RequestErrors.unprocessableEntity response next ctx
                | Ok validatedParams ->
                    dbContext.Entry(user).State <- EntityState.Detached

                    validatedParams
                    |> User.userOfAllowedParams
                    |> dbContext.Users.Update
                    |> ignore

                    let! _ = dbContext.SaveChangesAsync()
                    return! Successful.NO_CONTENT next ctx
            | Error _ ->
                return!
                    RequestErrors.notFound (json {| error = "Not Found" |}) next ctx
        }


let router : HttpHandler =
    choose [ POST >=> route "/" >=> create
             GET >=> route "/" >=> authorize >=> show
             PATCH >=> route "/" >=> authorize >=> update ]
