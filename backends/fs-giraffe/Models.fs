namespace Trendy.Models

open EntityFrameworkCore.FSharp
open Microsoft.EntityFrameworkCore.Design
open Microsoft.Extensions.DependencyInjection

type DesignTimeServices() =
    interface IDesignTimeServices with
        member __.ConfigureDesignTimeServices(serviceCollection: IServiceCollection) =
            let fSharpServices =
                EFCoreFSharpServices() :> IDesignTimeServices

            fSharpServices.ConfigureDesignTimeServices serviceCollection
            ()
