module Trendy.Controllers.Common

open Microsoft.Extensions.Logging
open Giraffe

let notFoundResponse : HttpHandler =
    json {| error = "Not Found." |}

let handleMissingUser (log: ILogger) (loc: string) (errorMessage: string) =
    log.LogCritical("Error when finding user in {loc}: #{e}", loc, errorMessage)

    RequestErrors.UNAUTHORIZED "Basic" "App" (json {| error = "User not found." |})

let handleInvalidEntity errors =
    let response = {| errors = errors |} |> json
    RequestErrors.unprocessableEntity response
