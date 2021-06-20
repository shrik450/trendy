namespace Trendy.Contexts


open Microsoft.EntityFrameworkCore
open EntityFrameworkCore.FSharp.Extensions
open Trendy.Models.DatabaseTypes

module LinksContext =
    type LinksContext() =
        inherit DbContext()

        [<DefaultValue>]
        val mutable links: DbSet<Link>

        [<DefaultValue>]
        val mutable users: DbSet<User>

        member this.Links
            with get () = this.links
            and set (v) = this.links <- v

        member this.Users
            with get () = this.users
            and set (v) = this.users <- v

        override _.OnModelCreating builder =
            builder.RegisterOptionTypes()
            builder.Entity<User>()
                .HasIndex([|"Email"|])
                .IsUnique() |> ignore

        override _.OnConfiguring(options) =
            options.UseSqlite("Data Source = links.db")
            |> ignore
