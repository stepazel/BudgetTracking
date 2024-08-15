namespace BudgetTrackingApp.Controllers

open BudgetTrackingApp.Models.InsightsModel
open Microsoft.AspNetCore.Authorization
open Dapper

[<Authorize>]
type InsightsController() =
    inherit BaseController()
    
    member this.Index() =
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
        let categories = this.Conn.Query<Category>(
            query, {| UserId = this.userId |}) |> Seq.toList

        this.View({ Categories = categories })

