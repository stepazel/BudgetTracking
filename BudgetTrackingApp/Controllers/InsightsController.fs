namespace BudgetTrackingApp.Controllers

open BudgetTrackingApp.Models.InsightsModel
open Microsoft.AspNetCore.Authorization
open Dapper

[<Authorize>]
type InsightsController() =
    inherit BaseController()
    
    member this.Index() =
        let query = """
select c.name                                                                                                                                                        as Name,
       sum(e.amount)                                                                                                                                                 as Total,
       count(1)                                                                                                                                                      as Count,
       percentile_cont(0.5) within group (order by e.amount)                                                                                                         as Median,
       max(e.amount)                                                                                                                                                 as Max,
       min(e.amount)                                                                                                                                                 as Min,
       sum(case when extract(year from e.created) = extract(year from now()) then e.amount else 0 end)                                                               as YearTotal,
       count(case when extract(year from e.created) = extract(year from now()) then 1 end)                                                                           as YearCount,
       sum(case when extract(year from e.created) = extract(year from now()) and extract(month from e.created) = extract(month from now()) then e.amount else 0 end) as MonthTotal,
       count(case when extract(year from e.created) = extract(year from now()) and extract(month from e.created) = extract(month from now()) then 1 end)             as MonthCount,
       sum(case when extract(year from e.created) = extract(year from now()) and extract(week from e.created) = extract(week from now()) then e.amount else 0 end)   as WeekTotal,
       count(case when extract(year from e.created) = extract(year from now()) and extract(week from e.created) = extract(week from now()) then 1 end)               as WeekCount
from expenses e
         left join categories c on e.category_id = c.id
where e.user_id = @user_id
group by c.name
"""
        let categories = this.Conn.Query<Category>(
            query, {| UserId = this.userId |}) |> Seq.toList

        this.View({ Categories = categories })

