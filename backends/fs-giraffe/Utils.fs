namespace Trendy

open FSharp.Control.Tasks
open System.Threading.Tasks

module Utils =
    let optionOfNullable value =
        match box value with
        | null -> None
        | _ -> Some value

    let asyncOptionOfAsyncNullable (asyncValue: Task<'a>) =
        task {
            let! result = asyncValue
            return result |> optionOfNullable
        }

    let resultOfNullable message value =
        match box value with
        | null -> Error message
        | _ -> Ok value

    let valueOrFallback value fallback =
        match box value with
        | null -> fallback
        | _ -> value

    let resultOfOption errorMessage opt =
        match opt with
        | Some value -> Ok value
        | None -> Error errorMessage

    let asyncify f a = task { return f a }

    let resultify (errorMessage: 'a) (opt: 'b -> 'c option) (arg: 'b) =
        arg |> opt |> resultOfOption errorMessage

    // The following definition is from Scott Wlaschin's "Railway Oriented
    // Programming" post, with the operator changed. Giraffe reserves the
    // >=> operator for its own use, so this one is "fishbone with a head",
    // >=>>, a compromise which uses F#'s inbuilt Result type instead of
    // Wlaschin's own.

    /// Combines functions which return result types to take one input
    /// and generate a result output.
    let (>=>>) switch1 switch2 x =
        match switch1 x with
        | Ok s -> switch2 s
        | Error f -> Error f

    /// An async version of the >=>> operator.
    let (>~>)
        (switch1: 'a -> Task<Result<'b, 'c>>)
        (switch2: 'b -> Task<Result<'d, 'c>>)
        x =
            task {
                match! switch1 x with
                | Ok s -> return! switch2 s
                | Error f -> return Error f
            }
