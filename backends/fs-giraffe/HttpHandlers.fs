namespace Trendy

module HttpHandlers =
    open Giraffe

    open Trendy.Controllers

    let router : HttpHandler =
        choose [ subRoute "/links" (LinksController.router)
                 subRoute "/users" (UsersController.router)
                 subRoute "/sessions" (SessionsController.router) ]
