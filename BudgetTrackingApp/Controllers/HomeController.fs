namespace BudgetTrackingApp.Controllers

open System
open System.Diagnostics
open BudgetTrackingApp.Repositories
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open BudgetTrackingApp.Models
open Npgsql

[<Authorize>]
type HomeController(logger: ILogger<HomeController>) =
    inherit Controller()
   
  
    member this.Index() =
        let userIdOption = this.HttpContext.User.Claims |> Seq.tryFind(fun claim -> claim.Type = "Id")

        let connection = UserRepository.connection
        let command = connection.CreateCommand()

        command.CommandText <- @"select description, amount, created from expenses where user_id = @user_id"
        command.Parameters.AddWithValue("user_id", Convert.ToInt32(userIdOption.Value.Value)) |> ignore
        use reader = command.ExecuteReader()
        let results =
            [while reader.Read() do
                 yield { Description = reader.GetString(0); Amount = reader.GetInt32(1); Created = reader.GetDateTime(2) }]
        reader.Close()
        command.CommandText <- @"select distinct description from expenses"
        use reader = command.ExecuteReader()
        let knownExpenses =
            [while reader.Read() do
                 yield {Description = reader.GetString(0) }]
            
        this.View({Expenses = results; KnownExpenses = knownExpenses })

    member this.AddExpense(description: string, amount: int, category: string) =
        let connection = UserRepository.connection
        let command = connection.CreateCommand()
        
        command.CommandText <- @"insert into expenses (description, amount, created, category, user_id)
                 values (@description, @amount, CURRENT_TIMESTAMP, @category, @user_id)"
        let userIdOption = this.HttpContext.User.Claims |> Seq.tryFind(fun claim -> claim.Type = "Id")
        command.Parameters.AddWithValue("@description", description) |> ignore
        command.Parameters.AddWithValue("@amount", amount) |> ignore
        command.Parameters.AddWithValue("@category", category) |> ignore
        command.Parameters.AddWithValue("@user_id", Convert.ToInt32(userIdOption.Value.Value)) |> ignore
        
        let _ = command.ExecuteNonQuery()
        this.RedirectToAction("Index")

    member this.Privacy() = this.View()

    [<ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)>]
    member this.Error() =
        let reqId =
            if isNull Activity.Current then
                this.HttpContext.TraceIdentifier
            else
                Activity.Current.Id

        this.View({ RequestId = reqId })
