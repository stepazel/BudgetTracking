namespace BudgetTrackingApp.Controllers

open System
open System.Diagnostics
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open BudgetTrackingApp.Models
open Npgsql

type HomeController(logger: ILogger<HomeController>) =
    inherit Controller()
   
  
    member this.Index() =
        let databasePassword = Environment.GetEnvironmentVariable("DATABASE_PASSWORD")
        let connectionString = $"Server=app-5ff20bb5-2de7-465b-8f9e-93cf3c184e3f-do-user-15095197-0.c.db.ondigitalocean.com;Port=25060;Database=db;User Id=db;Password={databasePassword};"
        use connection = NpgsqlDataSource.Create(connectionString)
        let command = connection.CreateCommand()

        command.CommandText <- @"select description, amount, created from expenses"
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
        use connection = NpgsqlDataSource.Create(Environment.GetEnvironmentVariable("DATABASE_URL"))
        let command = connection.CreateCommand()
        
        command.CommandText <- @"insert into expenses (description, amount, created, category)
                 values ($description, $amount, CURRENT_TIMESTAMP, $category)"
                 
        command.Parameters.AddWithValue("$description", description) |> ignore
        command.Parameters.AddWithValue("$amount", amount) |> ignore
        command.Parameters.AddWithValue("$category", category) |> ignore
        
        // connection.Open()
        let _ = command.ExecuteNonQuery()
        // connection.Close()
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
