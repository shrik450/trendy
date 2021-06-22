namespace Trendy
open FSharp.Control.Tasks
open System.Threading.Tasks

module Utils =
    let optionOfNullablle value =
        match box value with
        | null -> None
        | _ -> Some value
    
    let optionTaskOfNullableTask (asyncValue : Task<'a>) =
        task {
            let! result = asyncValue
            return result |> optionOfNullablle
        }

    let optionTaskOfNullableValueTask (asyncValue : ValueTask<'a>) =
        task {
            let! result = asyncValue
            return result |> optionOfNullablle
        }

    let optionAsyncOfNullableAsync asyncValue =
        async {
            let! result = asyncValue
            return result |> optionOfNullablle
        }
