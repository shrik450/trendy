namespace Trendy.Models

open BCrypt.Net

module User =
    type T = DatabaseTypes.User

    type AllowedParams =
        { Id: int
          Name: string
          Email: string
          Password: string
          ConfirmPassword: string }

    let userOfAllowedParams (allowedParams : AllowedParams) : T =
        {
            Id = allowedParams.Id
            Name = allowedParams.Name
            Email = allowedParams.Email
            EncryptedPassword = allowedParams.Password |> BCrypt.EnhancedHashPassword
            Links = []
        }
