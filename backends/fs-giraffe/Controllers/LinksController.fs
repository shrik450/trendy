namespace Trendy.Controllers

open Giraffe


module LinksController =
    let handleHello = text "Hello!"

    let router : HttpHandler =
        choose [ GET >=> route "/hello" >=> handleHello ]
