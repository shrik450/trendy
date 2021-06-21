namespace Trendy.Models

open System.ComponentModel.DataAnnotations

module DatabaseTypes =

    [<CLIMutable>]
    type Link =
        { [<Key>]
          Id: int
          [<Required>]
          Url: string
          Notes: string
          [<Required>]
          User: User }
    
    and [<CLIMutable>] 
    User =
        {
            [<Key>]
            Id: int
            [<Required>]
            Name: string
            // Also has an index and uniqueness defined via the fluent API
            // Check the context for more details.
            [<Required>]
            Email: string
            [<Required>]
            EncryptedPassword: string
            
            mutable Links: Link list
        }
    