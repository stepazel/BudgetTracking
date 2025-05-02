namespace BudgetTrackingApp.Controllers

open Microsoft.AspNetCore.Authorization
open Dapper
open BudgetTrackingApp.Models.Expenses

[<Authorize>]
type ExpensesController() =
    inherit BaseController()

    member this.Index() =
        let expenses = this.Conn.Query<Expense>("
select e.id, description, amount, created, c.name as CategoryName
from expenses e join categories c on e.category_id = c.id
where e.user_id = @UserId order by created desc;", {| UserId = this.userId |})
        this.View({Expenses = expenses})

