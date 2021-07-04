module Trendy.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe

open Microsoft.IdentityModel.Tokens
open Trendy.Controllers.Common
open Trendy.HttpHandlers
open Trendy.Contexts
open Trendy.Configuration
open Microsoft.AspNetCore.Authentication.JwtBearer
open System.Text

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose [ router
             setStatusCode 404 >=> notFoundResponse ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")

    clearResponse
    >=> setStatusCode 500
    >=> text "An error occurred while processing this request."

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder: CorsPolicyBuilder) =
    builder
        .WithOrigins("http://localhost:5000", "https://localhost:5001")
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore

let configureApp (app: IApplicationBuilder) =
    let env =
        app.ApplicationServices.GetService<IWebHostEnvironment>()

    (match env.IsDevelopment() with
     | true -> app.UseDeveloperExceptionPage()
     | false -> app.UseGiraffeErrorHandler(errorHandler))
        .UseAuthentication()
        .UseCors(configureCors)
        .UseGiraffe(webApp)

let jwtBearerOptions (cfg: JwtBearerOptions) =
    let config = ConfigStore().Config

    cfg.SaveToken <- true
    cfg.IncludeErrorDetails <- true
    cfg.TokenValidationParameters <- TokenValidationParameters()
    cfg.TokenValidationParameters.ValidateIssuer <- true
    cfg.TokenValidationParameters.ValidateAudience <- true
    cfg.TokenValidationParameters.ValidateLifetime <- true
    cfg.TokenValidationParameters.ValidateIssuerSigningKey <- true
    cfg.TokenValidationParameters.ValidIssuer <- config.Authorization.Issuer
    cfg.TokenValidationParameters.ValidAudience <- config.Authorization.Issuer

    cfg.TokenValidationParameters.IssuerSigningKey <-
        SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Authorization.Key))

let configureServices (services: IServiceCollection) =
    services
        .AddCors()
        .AddGiraffe()
        .AddDbContext<LinksContext.LinksContext>()
        .AddSingleton<IConfigStore, ConfigStore>() |> ignore

    services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(Action<JwtBearerOptions> jwtBearerOptions) |> ignore

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main args =
    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    0
