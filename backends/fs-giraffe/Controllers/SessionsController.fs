namespace Trendy.Controllers

open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Giraffe

open Trendy
open Trendy.Contexts
open Trendy.Services
open Trendy.Configuration
open Microsoft.IdentityModel.Tokens
open System.Text
open System.IdentityModel.Tokens.Jwt
open System
open System.Security.Claims
open BCrypt.Net

module SessionsController =
    open LinksContext
    open Trendy.Models
    open Trendy.Utils

    type BodyParams = { Email: string; Password: string }

    let userOfBodyParams (dbContext: LinksContext) { Email = email } =
        query {
            for user in dbContext.Users do
                where (user.Email = email)
                select user
                exactlyOneOrDefault
        }
        |> Utils.resultOfNullable

    let authenticateUser
        ({Password = password}: BodyParams)
        (user : User.T) =
            match BCrypt.EnhancedVerify(password, user.EncryptedPassword) with
            | true -> Ok user
            | false -> Error "Unauthorized"


    let createJwtToken config (user: User.T) =
        let securityKey =
            SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Authorization.Key))

        let credentials =
            SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)

        let claims =
            [ Claim(JwtRegisteredClaimNames.Email, user.Email) ]

        JwtSecurityToken(
            config.Authorization.Issuer,
            config.Authorization.Issuer,
            claims,
            DateTime.Now,
            DateTime.Now.AddMinutes(180.0),
            credentials
        )
        |> JwtSecurityTokenHandler().WriteToken


    let create : HttpHandler =
        fun next ctx ->
            task {
                let dbContext = ctx.GetService<LinksContext>()
                let config = ctx.GetService<IConfigStore>().Config
                let! requestParams = ctx.BindJsonAsync<BodyParams>()

                let authenticate =
                    userOfBodyParams dbContext
                    >=>> authenticateUser requestParams

                match authenticate requestParams with
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
