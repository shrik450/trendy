namespace Trendy.Controllers

open FSharp.Control.Tasks
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http
open Giraffe

open Trendy
open Trendy.Contexts
open Trendy.Services

module UsersController =

    open LinksContext
    open Trendy.Models

    [<CLIMutable>]
    type GetParams = { Id: int }

    [<CLIMutable>]
    type PostParams =
        { Id: int
          Name: string
          Email: string
          Password: string
          ConfirmPassword: string }

    let userOfGetParams (context : LinksContext) (requestParams : GetParams) =
        requestParams.Id |> context.Users.FindAsync |> Utils.optionTaskOfNullableValueTask


    let userOfPostParams (requestParams : PostParams) : User.AllowedParams =
        {
            Id = -1
            Name = requestParams.Name
            Email = requestParams.Email
            Password = requestParams.Password
            ConfirmPassword = requestParams.ConfirmPassword
        }

    let show (requestParams: GetParams) : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let dbContext = ctx.GetService<LinksContext>()
                match! requestParams |> userOfGetParams dbContext with
                | Some user ->
                    return! Successful.ok (json user) next ctx
                | None ->
                    return! RequestErrors.NOT_FOUND "Not Found." next ctx
            }

    let create : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let logger = ctx.GetLogger<ILogger>()
                let dbContext = ctx.GetService<LinksContext>()
                let! requestParams = ctx.BindJsonAsync<PostParams>()

                let validationResult =
                    requestParams
                    |> userOfPostParams
                    |> UserValidation.validate dbContext

                match validationResult with
                | Error errors ->
                    return! RequestErrors.unprocessableEntity (json errors) next ctx
                | Ok validatedParams ->
                    validatedParams
                    |> User.userOfAllowedParams
                    |> dbContext.Users.Add
                    |> ignore

                    let! _ = dbContext.SaveChangesAsync()
                    return! Successful.CREATED "" next ctx
            }


    let router : HttpHandler =
        choose [ GET >=> routeBind<GetParams> "/{id}(/?)" show
                 POST >=> route "/" >=> create ]
