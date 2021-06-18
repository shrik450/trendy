namespace Trendy

module HttpHandlers =

    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks
    open Giraffe
    open Trendy.Models

    let handleGetHello =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let response = {
                    Text = "Hello world, from Giraffe!"
                }
                return! json response next ctx
            }