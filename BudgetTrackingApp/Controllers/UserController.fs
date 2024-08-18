namespace BudgetTrackingApp.Controllers

open System
open System.Security.Cryptography
open System.Text
open BudgetTrackingApp.Models.UserpageModel
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authentication.Cookies
open Dapper

open System.Security.Claims
open Microsoft.AspNetCore.Authentication

type UserController() =
    inherit BaseController()

    member this.Index() = this.View()

    member this.SignUp() = this.View()

    member this.SignUpPost(username: string, password: string) =
        use sha512 = SHA512.Create()

        let hashedPassword =
            ASCIIEncoding.UTF8.GetBytes password
            |> sha512.ComputeHash
            |> Seq.fold (fun hash byte -> hash + byte.ToString("x2")) ""

        this.Conn.Execute("insert into users (username, password) values (@username, @password)", {|Username = username; Password = hashedPassword|}) |> ignore
        this.RedirectToAction("index", "home")

    member this.Login(username: string, password: string) =
        use sha512 = SHA512.Create()

        let hashedPassword =
            ASCIIEncoding.UTF8.GetBytes password
            |> sha512.ComputeHash
            |> Seq.fold (fun hash byte -> hash + byte.ToString("x2")) ""

        let userOption = this.Conn.Query<User>("select id, password from users where username = @username", {| Username = username |}) |> Seq.tryHead

        match userOption with
        | None ->
            this.RedirectToAction("index")
        | Some user ->
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
        let existingCategories = this.Conn.Query<Category>("select id, name from categories")
        let userCategories = this.Conn.Query<Category>(
            "select id, name from categories c left join user_categories uc on c.id = uc.category_id where uc.user_id = @UserId;",
            {| UserId = this.userId; |})
        
        this.View({ExistingCategories = existingCategories; UserCategories = userCategories })
        
    [<Authorize>]
    [<HttpPost>]
    member this.AddCategory(categoryId: int) =
        this.Conn.Execute("insert into user_categories (user_id, category_id) values (@UserId, @CategoryId)",
                     {| UserId = this.userId; CategoryId = categoryId |}) |> ignore
        this.RedirectToAction("ManageUserCategories")
        
    [<Authorize>]
    [<HttpPost>]
    member this.RemoveCategory(categoryId: int) =
        this.Conn.Execute("delete from user_categories where user_id = @UserId and category_id = @CategoryId",
                     {| UserId = this.userId; CategoryId = categoryId |}) |> ignore
        this.RedirectToAction("ManageUserCategories")
        
    member this.CreateCategory(name: string) =
        let existsAlready = this.Conn.QuerySingle<bool>("select count(1) from categories where name = @Name", {| Name = name |})
        if existsAlready = false then
            this.Conn.Execute("insert into categories (name) values (@Name)", {| Name = name |}) |> ignore
        this.RedirectToAction("ManageUserCategories")
        
        
        
    
