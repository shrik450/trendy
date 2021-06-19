namespace Trendy.Contexts


open Microsoft.EntityFrameworkCore
open EntityFrameworkCore.FSharp.Extensions
open Trendy.Models.Link

module LinksContext =
    type LinksContext() =
        inherit DbContext()

        [<DefaultValue>]
        val mutable links: DbSet<Link>

        member this.Links
            with get () = this.links
            and set (v) = this.links <- v

        override _.OnModelCreating builder = builder.RegisterOptionTypes()

        override _.OnConfiguring(options) =
            options.UseSqlite("Data Source = links.db")
            |> ignore
