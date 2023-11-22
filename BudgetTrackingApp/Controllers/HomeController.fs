namespace BudgetTrackingApp.Controllers

open System
open System.Diagnostics
open BudgetTrackingApp.Repositories
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open BudgetTrackingApp.Models

[<Authorize>]
type HomeController(logger: ILogger<HomeController>) =
    inherit Controller()
    member this.userId = this.HttpContext.User.Claims |> Seq.tryFind (fun claim -> claim.Type = "Id") |> Option.get |> fun claim -> claim.Value |> Convert.ToInt32
    
    member this.Index() =
        let connection = UserRepository.connection
        let command = connection.CreateCommand()
        command.CommandText <- @"select description, amount, created, category from expenses where user_id = @user_id"
        command.Parameters.AddWithValue("user_id", this.userId) |> ignore
        use reader = command.ExecuteReader()
        let results: Expense seq =
            [ while reader.Read() do
                  yield
                      { Description = reader.GetString(0)
                        Amount = reader.GetInt32(1)
                        Created = reader.GetDateTime(2)
                        Category = reader.GetString(3) } ]
        reader.Close()

        let (=>) x y = x,y
        this.View(
            { Expenses = results |> Seq.groupBy (fun expense -> expense.Category) |> Map.ofSeq
              CategoryNames =
                Map
                    [
                      "groceries" => "Potraviny"
                      "food"=> "Jídlo (restaurace)"
                      "housing" => "Bydlení"
                      "drinks" => "Pití (zbytné)"
                      "transport" => "Doprava" ] }
        )

    member this.AddExpense(description: string, amount: int, category: string) =
        let connection = UserRepository.connection
        let command = connection.CreateCommand()

        command.CommandText <-
            @"insert into expenses (description, amount, created, category, user_id)
                 values (@description, @amount, CURRENT_TIMESTAMP, @category, @user_id)"

        command.Parameters.AddWithValue("@description", description) |> ignore
        command.Parameters.AddWithValue("@amount", amount) |> ignore
        command.Parameters.AddWithValue("@category", category) |> ignore

        command.Parameters.AddWithValue("@user_id", this.userId) |> ignore

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
