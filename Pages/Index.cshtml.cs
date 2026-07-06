using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using BudgetTrackingNew.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BudgetTrackingNew.Pages;

[Authorize]
public class IndexModel(IDbConnection dbConnection) : PageModel
{
    public List<Expense> Expenses { get; set; } = [];
    public List<Category> UserCategories { get; set; } = [];

    [BindProperty] public Expense NewExpense { get; set; } = new();
    

    public void OnGet()
    {
        
        Expenses = dbConnection
            .Query<Expense>(
                """
                select expenses.id, amount, description, created, inputted, category_id, categories.name as category_name
                from expenses
                left join categories on categories.id = expenses.category_id
                where expenses.user_id = @UserId
                order by created desc limit 10
                """, new { UserId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier)!) })
            .ToList();

        UserCategories = dbConnection
            .Query<Category>("""
                             select categories.id, name from categories
                                             left join user_categories on user_categories.category_id = categories.id
                                             where user_categories.user_id = @UserId
                             """,
                new { UserId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier)) }
            ).ToList();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        NewExpense.UserId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        NewExpense.Inputted = DateTime.Now;
        dbConnection.Execute(
            "INSERT INTO Expenses (Description, Amount, Created, Inputted, Category_Id, User_Id) VALUES (@Description, @Amount, @Created, @Inputted, @CategoryId, @UserId)",
            NewExpense);

        return RedirectToPage();
    }

    private void LoadExpenses()
    {
    }
}