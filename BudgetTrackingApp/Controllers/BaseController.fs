namespace BudgetTrackingApp.Controllers

open System
open BudgetTrackingApp.Repositories
open Microsoft.AspNetCore.Mvc

type BaseController() =
    inherit Controller()
    
    member this.userId =
        this.HttpContext.User.Claims
        |> Seq.tryFind (fun claim -> claim.Type = "Id")
        |> Option.get
        |> fun claim -> claim.Value |> Convert.ToInt32
        
    member this.Conn =
        ConnectionProvider.conn