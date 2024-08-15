namespace BudgetTrackingApp.Controllers
open BudgetTrackingApp.Repositories.DatabaseModels
open System
open System.Diagnostics
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Dapper

open BudgetTrackingApp.Models

module ActionResult =
    let ofAsync (res: Async<IActionResult>) =
        res |> Async.StartAsTask

[<Authorize>]
type HomeController() =
    inherit BaseController()

    member this.Index() =
        let totalsQuery = """
        select
            coalesce(sum(amount), 0) as Total,
            coalesce(sum(case when extract(year from created) = extract(year from now()) then amount else 0 end), 0) as YearlyTotal,
            coalesce(sum(case when extract(year from created) = extract(year from now()) and extract(month from created) = extract(month from now()) then amount else 0 end), 0) as MonthlyTotal,
            coalesce(sum(case when extract(year from created) = extract(year from now()) and extract(week from created) = extract(week from now()) then amount else 0 end), 0) as WeeklyTotal
        from expenses
        where user_id = @UserId
        """
        let totals = this.Conn.QuerySingle<TotalsResult>(totalsQuery, {| UserId = this.userId |})

        let categories = this.Conn.Query<Category>(
            "select id, name from categories c left join user_categories uc on c.id = uc.category_id where uc.user_id = @UserId;",
            {| UserId = this.userId |})
        let categoryNamesMap = categories |> Seq.map (fun c  -> (c.Id, c.Name)) |> Map.ofSeq
        
        let last5expenses = this.Conn.Query<Expense>("
select e.id, description, amount, created, c.name as CategoryName
from expenses e join categories c on e.category_id = c.id
where e.user_id = @UserId order by created desc limit 5;", {| UserId = this.userId |})

        this.View({
            Expenses = last5expenses
            Categories = categoryNamesMap
            Total = totals.Total
            YearlyTotal = totals.YearlyTotal
            MonthlyTotal = totals.MonthlyTotal
            WeeklyTotal = totals.WeeklyTotal
        })
        
        
    member this.AddExpense(description: string, amount: int, categoryId: int, date: DateTime) =
        let expense =
            { Id = Nullable()
              Description = description
              Amount = amount
              Created = date
              CategoryId = categoryId
              UserId = this.userId }
 
        let sql = """
         INSERT INTO expenses (category_id, amount, description, created, user_id)
         VALUES (@CategoryId, @Amount, @Description, @Created, @UserId);
        """
        this.Conn.Execute(sql, expense) |> ignore
        this.RedirectToAction("Index")

    member this.Privacy() = this.View()
            
    [<HttpDelete>]
    member this.DeleteExpense(id: int) =
        this.Conn.Execute("delete from expenses where id = @Id and user_id = @UserId", {| Id = id; UserId = this.userId |}) |> ignore
        this.RedirectToAction("index")

    [<ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)>]
    member this.Error() =
        let reqId =
            if isNull Activity.Current then
                this.HttpContext.TraceIdentifier
            else
                Activity.Current.Id

        this.View({ RequestId = reqId })
