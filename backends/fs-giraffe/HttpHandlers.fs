namespace Trendy

module HttpHandlers =
    open Giraffe

    open Trendy.Controllers

    let router : HttpHandler =
        choose [ subRoute "/links" (LinksController.router) ]
