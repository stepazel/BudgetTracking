namespace BudgetTrackingApp.Controllers

open System.Diagnostics

open Microsoft.AspNetCore.Mvc
open Microsoft.Data.Sqlite
open Microsoft.Extensions.Logging

open BudgetTrackingApp.Models

type HomeController(logger: ILogger<HomeController>) =
    inherit Controller()

    member this.Index() = this.View()

    member this.AddExpense(description: string, amount: int) =
        use connection = new SqliteConnection("Data source=app.db")
        let command = connection.CreateCommand()

        command.CommandText <-
            @"insert into expenses (description, amount, created)
                values ($description, $amount, CURRENT_TIMESTAMP)"
                
        command.Parameters.AddWithValue("$description", description)
        command.Parameters.AddWithValue("$amount", amount)
        
        connection.Open()
        let executeNonQuery = command.ExecuteNonQuery()
        connection.Close()
        this.View("index")

    member this.Privacy() = this.View()

    [<ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)>]
    member this.Error() =
        let reqId =
            if isNull Activity.Current then
                this.HttpContext.TraceIdentifier
            else
                Activity.Current.Id

        this.View({ RequestId = reqId })
