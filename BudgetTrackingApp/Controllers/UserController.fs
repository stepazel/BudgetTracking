namespace BudgetTrackingApp.Controllers

open System
open System.Security.Cryptography
open System.Text
open BudgetTrackingApp.Models.InsightsModel
open BudgetTrackingApp.Models.UserpageModel
open BudgetTrackingApp.Repositories
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Authentication.Cookies
open Dapper

open System.Security.Claims
open Microsoft.AspNetCore.Authentication

type UserController(logger: ILogger<UserController>) =
    inherit Controller()

    member this.userId =
        this.HttpContext.User.Claims
        |> Seq.tryFind (fun claim -> claim.Type = "Id")
        |> Option.get
        |> fun claim -> claim.Value |> Convert.ToInt32
    member this.Index() = this.View()

    member this.SignUp() = this.View()

    member this.SignUpPost(username: string, password: string) =
        use sha512 = SHA512.Create()

        let hashedPassword =
            ASCIIEncoding.UTF8.GetBytes password
            |> sha512.ComputeHash
            |> Seq.fold (fun hash byte -> hash + byte.ToString("x2")) ""

        let connection = ConnectionProvider.connection
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

        let connection = ConnectionProvider.connection
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
        
    [<Authorize>]
    member this.ManageUserCategories() =
        let conn = ConnectionProvider.conn
        let existingCategories = conn.Query<Category>("select id, name from categories")
        let userCategories = conn.Query<Category>(
            "select id, name from categories c left join user_categories uc on c.id = uc.category_id where uc.user_id = @UserId;",
            {| UserId = this.userId; |})
        
        this.View({ExistingCategories = existingCategories; UserCategories = userCategories })
        
    [<Authorize>]
    [<HttpPost>]
    member this.AddCategory(categoryId: int) =
        let conn = ConnectionProvider.conn
        conn.Execute("insert into user_categories (user_id, category_id) values (@UserId, @CategoryId)",
                     {| UserId = this.userId; CategoryId = categoryId |}) |> ignore
        this.RedirectToAction("ManageUserCategories")
        
    [<Authorize>]
    [<HttpPost>]
    member this.RemoveCategory(categoryId: int) =
        let conn = ConnectionProvider.conn
        conn.Execute("delete from user_categories where user_id = @UserId and category_id = @CategoryId",
                     {| UserId = this.userId; CategoryId = categoryId |}) |> ignore
        this.RedirectToAction("ManageUserCategories")
        
    member this.CreateCategory(name: string) =
        let conn = ConnectionProvider.conn
        let existsAlready = conn.QuerySingle<bool>("select count(1) from categories where name = @Name", {| Name = name |})
        if existsAlready = false then
            conn.Execute("insert into categories (name) values (@Name)", {| Name = name |}) |> ignore
        this.RedirectToAction("ManageUserCategories")
        
        
        
    
