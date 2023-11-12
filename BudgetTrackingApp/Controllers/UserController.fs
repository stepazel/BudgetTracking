namespace BudgetTrackingApp.Controllers.UserController

open System.Security.Cryptography
open System.Text
open Microsoft.AspNetCore.Mvc
open Microsoft.Data.Sqlite
open Microsoft.Extensions.Logging

type UserController (logger: ILogger<UserController>) =
    inherit Controller()    

    member this.Index() =
        this.View()
        
    member this.SignUp(username: string, password: string) =
        use sha512 = SHA512.Create()
        let hashedPassword = ASCIIEncoding.UTF8.GetBytes password |> sha512.ComputeHash |> Seq.fold (fun hash byte -> hash + byte.ToString("x2")) ""
        use connection = new SqliteConnection("Data source=app.db")
        let command = connection.CreateCommand()
        command.CommandText <- "insert into users (username, password) values ($username, $password)"
        command.Parameters.AddWithValue("$username", username) |> ignore
        command.Parameters.AddWithValue("$password", hashedPassword) |> ignore
        
        connection.Open()
        command.ExecuteNonQuery() |> ignore
        connection.Close()

        this.RedirectToAction("index", "home")
        
