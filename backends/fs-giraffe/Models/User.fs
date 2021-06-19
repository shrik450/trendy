namespace Trendy.Models

module User =
    open Microsoft.AspNetCore.Identity

    type User() =
        inherit IdentityUser<int>()
