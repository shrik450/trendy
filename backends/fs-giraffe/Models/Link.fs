namespace Trendy.Models

open System.ComponentModel.DataAnnotations

module Link =
    [<CLIMutable>]
    type Link =
        { [<Key>]
          Id: int
          Url: string
          Notes: string }
