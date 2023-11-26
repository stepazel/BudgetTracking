namespace BudgetTrackingApp.Controllers

open System
open System.Diagnostics
open System.Net.Http
open BudgetTrackingApp.Repositories
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open BudgetTrackingApp.Models

[<Authorize>]
type HomeController(logger: ILogger<HomeController>) =
    inherit Controller()

    member this.userId =
        this.HttpContext.User.Claims
        |> Seq.tryFind (fun claim -> claim.Type = "Id")
        |> Option.get
        |> fun claim -> claim.Value |> Convert.ToInt32

    member this.Index() =
        let connection = UserRepository.connection
        let command = connection.CreateCommand()
        command.CommandText <- @"select description, amount, created, category, id from expenses where user_id = @user_id order by created desc"
        command.Parameters.AddWithValue("user_id", this.userId) |> ignore
        use reader = command.ExecuteReader()

        let results: Expense seq =
            [ while reader.Read() do
                  yield
                      { Description = reader.GetString(0)
                        Amount = reader.GetInt32(1)
                        Created = reader.GetDateTime(2)
                        Category = reader.GetString(3)
                        Id = reader.GetInt32(4) } ]

        reader.Close()

        let (=>) x y = x, y

        this.View(
            { Expenses = results |> Seq.groupBy (fun expense -> expense.Category) |> Map.ofSeq
              CategoryNames =
                Map
                    [ "groceries" => "Potraviny"
                      "food" => "Jídlo (restaurace)"
                      "housing" => "Bydlení"
                      "drinks" => "Pití (zbytné)"
                      "transport" => "Doprava"
                      "leisure" => "Zábava" ]
              Total = results |> Seq.sumBy (fun expense -> expense.Amount)  }
        )

    member this.AddExpense(description: string, amount: float, category: string, currency: string) =
        let connection = UserRepository.connection
        let command = connection.CreateCommand()

        command.CommandText <-
            @"insert into expenses (description, amount, created, category, user_id)
                 values (@description, @amount, CURRENT_TIMESTAMP, @category, @user_id)"
        
        // TODO https://cdn.jsdelivr.net/gh/fawazahmed0/currency-api@1/latest/currencies/eur/czk.json

        // create the HttpClient
        let client = new HttpClient()
        let async = client.GetAsync("https://cdn.jsdelivr.net/gh/fawazahmed0/currency-api@1/latest/currencies/eur/czk.json")
        // get the response
        let response = async.Result
// get the response content
        
                
        command.Parameters.AddWithValue("@description", description) |> ignore
        if currency = "EUR" then
            command.Parameters.AddWithValue("@amount", round (amount * 24.5)) |> ignore
            else
            command.Parameters.AddWithValue("@amount", amount) |> ignore
        command.Parameters.AddWithValue("@category", category) |> ignore

        command.Parameters.AddWithValue("@user_id", this.userId) |> ignore

        let _ = command.ExecuteNonQuery()
        this.RedirectToAction("Index")

    member this.Privacy() = this.View()

    [<HttpPost>]
    member this.EditExpense(id: int, description: string, amount: int, category: string) =
        let command = UserRepository.connection.CreateCommand()

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
        let command = UserRepository.connection.CreateCommand()
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
