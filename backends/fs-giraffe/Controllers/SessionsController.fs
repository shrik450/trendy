module Trendy.Controllers.SessionsController

open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Giraffe

open Microsoft.Extensions.Logging
open Trendy
open Trendy.Contexts.LinksContext
open Trendy.Services.Authentication
open Trendy.Configuration
open Trendy.Models
open Trendy.Utils

type BodyParams = { Email: string; Password: string }

let userOfBodyParams (dbContext: LinksContext) { Email = email } =
    User.findByEmailAsync dbContext email

let create : HttpHandler =
    fun next ctx ->
        task {
            let dbContext = ctx.GetService<LinksContext>()
            let config = ctx.GetService<IConfigStore>().Config
            let! requestParams = ctx.BindJsonAsync<BodyParams>()
            let safePassword = valueOrFallback requestParams.Password ""

            let authenticate =
                userOfBodyParams dbContext
                >~> ((authenticateUser safePassword) |> asyncify)

            match! authenticate requestParams with
            | Ok user ->
                let token = createJwtToken config user
                return! Successful.ok (json {| token = token |}) next ctx
            | Error _ ->
                return!
                    RequestErrors.unauthorized
                        "Basic"
                        "App"
                        (json {| error = "Invalid username or password." |})
                        next
                        ctx
        }

let router : HttpHandler = choose [ POST >=> route "/" >=> create ]
