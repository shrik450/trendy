module Trendy.Configuration

open Microsoft.Extensions.Configuration
open System.IO
open FsConfig

type AuthorizationConfig = { Key: string; Issuer: string }

type Config = { Authorization: AuthorizationConfig }

type IConfigStore =
    abstract Config: Config

// I am not 100% sure of what I've done here.
type ConfigStore(config : Config) =
    let mutable config = config

    /// Argument-less constructor for ConfigStore that loads a configuration
    /// from appsettings.json
    new() =
        let configurationRoot =
            ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build()

        let appConfig () =
            match AppConfig(configurationRoot).Get<Config>() with
            | Ok config -> config
            | Error _ -> failwith "Invalid configuration."
        ConfigStore(appConfig ())

    interface IConfigStore
        // Why is config not a member of ConfigStore? Otherwise I'd need
        // the this.
        with member this.Config with get() = config

    // ???????????
    member this.Config with get() = config


