module Trendy.Services.Authentication

open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication.JwtBearer
open FSharp.Control.Tasks
open Giraffe

open Trendy.Utils
open Trendy.Models
open Trendy.Contexts.LinksContext

let authorize : HttpHandler =
    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let emailClaim (ctx: HttpContext) =
        let claims = ctx.User.Claims
        query {
            for claim in claims do
                where (claim.Type = JwtRegisteredClaimNames.Email)
                select claim
                exactlyOneOrDefault
        }
        |> resultOfNullable "No email Claim found in JWT."

let emailOfClaim (claim: Claim) = Ok claim.Value

let currentUser (ctx: HttpContext) =
    let dbContext = ctx.GetService<LinksContext>()

    ctx
    |> (((emailClaim >=>> emailOfClaim) |> asyncFOfSyncF)
        >~> User.findByEmailAsync dbContext)
