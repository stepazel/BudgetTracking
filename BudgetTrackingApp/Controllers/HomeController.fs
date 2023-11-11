namespace BudgetTrackingApp.Controllers

open System.Diagnostics

open Microsoft.AspNetCore.Mvc
open Microsoft.Data.Sqlite
open Microsoft.Extensions.Logging

open BudgetTrackingApp.Models

type HomeController(logger: ILogger<HomeController>) =
    inherit Controller()
   
  
    member this.Index() =
        use connection = new SqliteConnection("Data source=app.db")
        let command = connection.CreateCommand()

        command.CommandText <- @"select description, amount, created from expenses"
        connection.Open()
        use reader = command.ExecuteReader()
        let results =
            [while reader.Read() do
                 yield { Description = reader.GetString(0); Amount = reader.GetInt32(1); Created = reader.GetDateTime(2) }]
            
        connection.Close()
        this.View({Expenses = results})

    member this.AddExpense(description: string, amount: int) =
        use connection = new SqliteConnection("Data source=app.db")
        let command = connection.CreateCommand()
        
        command.CommandText <- @"insert into expenses (description, amount, created)
                 values ($description, $amount, CURRENT_TIMESTAMP)"
                 
        command.Parameters.AddWithValue("$description", description) |> ignore
        command.Parameters.AddWithValue("$amount", amount) |> ignore
        
        connection.Open()
        let _ = command.ExecuteNonQuery()
        connection.Close()
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
