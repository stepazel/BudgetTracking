namespace BudgetTrackingApp.Controllers.UserController

open System
open System.Security.Cryptography
open System.Text
open BudgetTrackingApp.Models.UserpageModel
open BudgetTrackingApp.Repositories
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Authentication.Cookies

open System.Security.Claims
open Microsoft.AspNetCore.Authentication

type UserController(logger: ILogger<UserController>) =
    inherit Controller()

    member this.Index() = this.View()

    member this.SignUp() = this.View()

    member this.SignUpPost(username: string, password: string) =
        use sha512 = SHA512.Create()

        let hashedPassword =
            ASCIIEncoding.UTF8.GetBytes password
            |> sha512.ComputeHash
            |> Seq.fold (fun hash byte -> hash + byte.ToString("x2")) ""

        let connection = UserRepository.connection
        let command = connection.CreateCommand()
        command.CommandText <- "insert into users (username, password) values (@username, @password)"
        command.Parameters.AddWithValue("username", username) |> ignore
        command.Parameters.AddWithValue("password", hashedPassword) |> ignore
        command.ExecuteNonQuery() |> ignore

        this.RedirectToAction("index", "home")

    member this.Login(username: string, password: string) =
        use sha512 = SHA512.Create()

        let hashedPassword =
            ASCIIEncoding.UTF8.GetBytes password
            |> sha512.ComputeHash
            |> Seq.fold (fun hash byte -> hash + byte.ToString("x2")) ""

        let connection = UserRepository.connection
        let command = connection.CreateCommand()
        command.CommandText <- "select id, password from users where username = @username"
        command.Parameters.AddWithValue("username", username) |> ignore
        use reader = command.ExecuteReader()

        let results =
            [ while reader.Read() do
                  yield
                      { Id = reader.GetInt32(0)
                        Password = reader.GetString(1) } ]

        reader.Close()

        if results.IsEmpty then
            this.RedirectToAction("index")
        else
            let user: User = results.Head

            if (user.Password = hashedPassword) then
                let claims = [ Claim(ClaimTypes.Name, username); Claim("Id", string user.Id) ]

                let claimIdentity =
                    ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)

                let redirectToActionResult = this.RedirectToAction("index", "home")

                let signInTask =
                    (async {
                        let _ =
                            this.HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                ClaimsPrincipal(claimIdentity),
                                AuthenticationProperties()
                            )

                        this.RedirectToAction("index", "home") |> ignore
                    })

                Async.RunSynchronously(signInTask)
                redirectToActionResult
            else
                this.RedirectToAction("index")

    [<Authorize>]
    member this.Logout() =
        let signOutTask =
            (async {
                let _ = this.HttpContext.SignOutAsync()
                Console.WriteLine("logout")
            })

        Async.RunSynchronously(signOutTask)
        this.RedirectToAction("index")
