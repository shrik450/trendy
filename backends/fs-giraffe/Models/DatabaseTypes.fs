namespace Trendy.Models

open System.ComponentModel.DataAnnotations

module DatabaseTypes =
    open Microsoft.AspNetCore.Identity

    [<CLIMutable>]
    type Link =
        { [<Key>]
          Id: int
          [<Required>]
          Url: string
          Notes: string
          [<Required>]
          User: User }
    and User() =
        inherit IdentityUser<int>()
        
        [<DefaultValue>]
        val mutable links: Link list

        member this.Links with get () = this.links and set(v) = this.links <- v
    