namespace Trendy.Services

module Validation =
    /// The result of a validation. The Ok half is ignored. The Error
    /// half is key * error message.
    type ValidationResult = Result<unit, string * string>
    /// A function that validates an 'a value.
    type 'a Validator = 'a -> ValidationResult
    /// The result of running multiple validations on a value.
    type ValidationErrors = Map<string, string list>

    let validate
        (validations : 'a Validator list)
        (record : 'a) : ValidationErrors =
            List.fold
                (
                    fun errors validation ->
                        match validation record with
                        | Ok _ -> errors
                        | Error (key, value) ->
                            Map.change
                                key
                                (
                                    function
                                        | Some m -> Some (value::m)
                                        | None -> Some [value]
                                )
                                errors

                )
                Map.empty
                validations
