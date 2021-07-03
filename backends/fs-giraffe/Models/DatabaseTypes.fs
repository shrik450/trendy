module Trendy.Models.DatabaseTypes

open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema

[<CLIMutable>]
type Link =
    { [<Key>]
      Id: int
      [<Required>]
      Url: string
      Notes: string
      [<Required>]
      UserId: int }
    interface IPageable with
        member this.Id = this.Id

[<CLIMutable>]
type User =
    { [<Key>]
      [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
      Id: int
      [<Required>]
      Name: string
      // Also has an index and uniqueness defined via the fluent API
      // Check the context for more details.
      [<Required>]
      Email: string
      [<Required>]
      EncryptedPassword: string }
