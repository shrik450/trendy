namespace Trendy.Controllers

open Microsoft.AspNetCore.Http
open BCrypt.Net
open Giraffe

open Trendy.Contexts
open FSharp.Control.Tasks
open Microsoft.Extensions.Logging

module UsersController =

    open LinksContext
    open Trendy.Models

    [<CLIMutable>]
    type GetParams = { Id: int }

    [<CLIMutable>]
    type PostParams =
        { Name: string
          Email: string
          Password: string }

    let show (requestParams: GetParams) : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let dbContext = ctx.GetService<LinksContext>()

                let user = dbContext.Users.Find(requestParams.Id)

                match box user with
                | null -> return! (RequestErrors.NOT_FOUND "Not Found.") next ctx
                | _ -> return! Successful.ok (json user) next ctx
            }

    let create : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let logger = ctx.GetLogger<ILogger>()
                let dbContext = ctx.GetService<LinksContext>()
                let! requestParams = ctx.BindJsonAsync<PostParams>()

                let encryptedPassword =
                    BCrypt.EnhancedHashPassword(requestParams.Password)

                let newUser : User.T =
                    { Id = 0
                      Name = requestParams.Name
                      Email = requestParams.Email
                      EncryptedPassword = encryptedPassword
                      Links = [] }

                dbContext.Users.Add(newUser) |> ignore

                try
                    let! _ = dbContext.SaveChangesAsync()
                    return! Successful.created (json newUser) next ctx
                with
                | _ ->
                    logger.LogCritical("Failed to save to db")
                    return! (RequestErrors.UNPROCESSABLE_ENTITY "Error while saving User") next ctx
            }


    let router : HttpHandler =
        choose [ GET >=> routeBind<GetParams> "/{id}(/?)" show
                 POST >=> route "/" >=> create ]
