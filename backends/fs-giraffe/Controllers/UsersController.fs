namespace Trendy.Controllers

open FSharp.Control.Tasks
open Microsoft.Extensions.Logging
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open BCrypt.Net
open Giraffe

open Trendy
open Trendy.Contexts

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

    let userOfGetParams (context : LinksContext) (requestParams : GetParams) =
        requestParams.Id |> context.Users.FindAsync |> Utils.optionTaskOfNullableValueTask

    
    let userOfPostParams (requestParams : PostParams) : User.T =
        {
            Id = 0
            Name = requestParams.Name
            Email = requestParams.Email
            EncryptedPassword = requestParams.Password |> BCrypt.EnhancedHashPassword
            Links = []
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

                requestParams 
                |> userOfPostParams 
                |> dbContext.Users.Add 
                |> ignore

                try
                    let! _ = dbContext.SaveChangesAsync()
                    return! Successful.CREATED "" next ctx
                with
                | _ ->
                    logger.LogCritical("Failed to save to db")
                    return! (RequestErrors.UNPROCESSABLE_ENTITY "Error while saving User") next ctx
            }


    let router : HttpHandler =
        choose [ GET >=> routeBind<GetParams> "/{id}(/?)" show
                 POST >=> route "/" >=> create ]
