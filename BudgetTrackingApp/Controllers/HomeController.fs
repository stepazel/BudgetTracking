namespace BudgetTrackingApp.Controllers
open BudgetTrackingApp.Repositories.DatabaseModels
open System
open System.Diagnostics
open BudgetTrackingApp.Repositories
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Dapper

open BudgetTrackingApp.Models

module ActionResult =
    let ofAsync (res: Async<IActionResult>) =
        res |> Async.StartAsTask

[<Authorize>]
type HomeController(logger: ILogger<HomeController>) =
    inherit Controller()

    member this.userId =
        this.HttpContext.User.Claims
        |> Seq.tryFind (fun claim -> claim.Type = "Id")
        |> Option.get
        |> fun claim -> claim.Value |> Convert.ToInt32

    member this.Index() =
        ActionResult.ofAsync <| async {
            let conn = ConnectionProvider.conn
            let categories = conn.Query<Category>("select id, name from categories")
            let categoryNamesMap = categories |> Seq.map (fun c  -> (c.Id, c.Name)) |> Map.ofSeq

            let total = conn.QuerySingle<float>("""select coalesce(sum(amount), 0) from expenses where user_id = @UserId""", {| UserId = this.userId |})
                
            let yearlyTotal = conn.QuerySingle<float>("""select coalesce(sum(amount), 0) from expenses 
                     where extract(year from created) = extract(year from now())
                       and user_id = @UserId""", {| UserId = this.userId |})
                
            let monthlyTotal = conn.QuerySingle<float>(""" select coalesce(sum(amount), 0) from expenses 
                     where extract(year from created) = extract(year from now()) 
                       and extract(month from created) = extract(month from now())
                       and user_id = @UserId""", {| UserId = this.userId|})
            
            let weeklyTotal = conn.QuerySingle<float>(""" select coalesce(sum(amount), 0) from expenses 
                     where extract(year from created) = extract(year from now()) 
                       and extract(month from created) = extract(month from now())
                       and extract(week from created) = extract(week from now())
                       and user_id = @UserId""", {| UserId = this.userId|})
            
            return this.View({
                Categories = categoryNamesMap
                Total = total
                YearlyTotal = yearlyTotal
                MonthlyTotal = monthlyTotal
                WeeklyTotal = weeklyTotal
            }) :> IActionResult
        } 
        
    member this.AddExpense(description: string, amount: int, categoryId: int, currency: string) =
        let conn = ConnectionProvider.conn
        
        let expense =
            { Id = Nullable()
              Description = description
              Amount = amount
              Created = DateTime.Now
              CategoryId = categoryId
              UserId = this.userId }
 
        let sql = """
         INSERT INTO expenses (category_id, amount, description, created, user_id)
         VALUES (@CategoryId, @Amount, @Description, @Created, @UserId);
        """
        conn.Execute(sql, expense) |> ignore;
        this.RedirectToAction("Index")

    member this.Privacy() = this.View()

    [<HttpPost>]
    member this.EditExpense(id: int, description: string, amount: int, category: string) =
        let command = ConnectionProvider.connection.CreateCommand()

        command.CommandText <- "select user_id from expenses where id = @id"
        command.Parameters.AddWithValue("@id", id) |> ignore

        let userId = command.ExecuteScalar() |> Convert.ToInt32

        if userId = this.userId then
            command.CommandText <- "update expenses set (description, amount, category) = (@description, @amount, @category) where id = @expense_id"
            command.Parameters.AddWithValue("@expense_id", id) |> ignore
            command.Parameters.AddWithValue("@description", description) |> ignore
            command.Parameters.AddWithValue("@amount", amount) |> ignore
            command.Parameters.AddWithValue("@category", category) |> ignore
            command.ExecuteNonQuery() |> ignore
            this.RedirectToAction("index")
        else
            this.RedirectToAction("index")
            
    [<HttpPost>]
    member this.DeleteExpense(id: int) =
        let command = ConnectionProvider.connection.CreateCommand()
        command.CommandText <- "delete from expenses where id = @id and user_id = @user_id"
        command.Parameters.AddWithValue("@id", id) |> ignore
        command.Parameters.AddWithValue("@user_id", this.userId) |> ignore
        command.ExecuteNonQuery() |> ignore
        this.RedirectToAction("index")

    [<ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)>]
    member this.Error() =
        let reqId =
            if isNull Activity.Current then
                this.HttpContext.TraceIdentifier
            else
                Activity.Current.Id

        this.View({ RequestId = reqId })
