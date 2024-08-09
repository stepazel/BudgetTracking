namespace BudgetTrackingApp.Controllers

open System
open BudgetTrackingApp.Models.InsightsModel
open BudgetTrackingApp.Repositories
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Dapper

[<Authorize>]
type InsightsController(logger: ILogger<InsightsController>) =
    inherit Controller()
    member this.userId =
        this.HttpContext.User.Claims
        |> Seq.tryFind (fun claim -> claim.Type = "Id")
        |> Option.get
        |> fun claim -> claim.Value |> Convert.ToInt32
    
    
    member this.Index() =
        let conn = ConnectionProvider.conn
       
        let query = """
select
    c.name as name,
    sum(case when extract(year from e.created) = extract(year from now()) then e.amount else 0 end) as yearlytotal,
    sum(case when extract(year from e.created) = extract(year from now())
        and extract(month from e.created) = extract(month from now()) then e.amount else 0 end) as monthlytotal,
    sum(case when extract(year from e.created) = extract(year from now())
        and extract(week from e.created) = extract(week from now()) then e.amount else 0 end) as weeklytotal
from expenses e
         left join categories c on e.category_id = c.id
where e.user_id = @user_id
group by c.name
order by yearlytotal desc
"""
        let categories = conn.Query<Category>(
            query, {| UserId = this.userId |}) |> Seq.toList

        this.View({ Categories = categories })

