module Trendy.Services.Authentication

open System
open System.Text
open System.Security.Claims
open System.IdentityModel.Tokens.Jwt
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication.JwtBearer
open BCrypt.Net
open Giraffe

open Microsoft.IdentityModel.Tokens
open Trendy.Configuration
open Trendy.Utils
open Trendy.Models
open Trendy.Contexts.LinksContext

let authorize : HttpHandler =
    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let authenticateUser (password : string) (user: User.T) =
    match BCrypt.EnhancedVerify(password, user.EncryptedPassword) with
    | true  -> Ok user
    | false -> Error "Incorrect Password."

let createJwtToken (config : Config) (user: User.T) =
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

let claimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"

let emailClaim (ctx: HttpContext) =
    let claims = ctx.User.Claims
    query {
        for claim in claims do
            where (claim.Type = claimType)
            select claim
            exactlyOneOrDefault
    }
    |> resultOfNullable "No email Claim found in JWT."

let emailOfClaim (claim: Claim) = Ok claim.Value

let currentUser (ctx: HttpContext) =
    let dbContext = ctx.GetService<LinksContext>()

    ctx
    |> (((emailClaim >=>> emailOfClaim) |> asyncify)
        >~> User.findByEmailAsync dbContext)
