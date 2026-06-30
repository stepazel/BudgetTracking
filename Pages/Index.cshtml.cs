using System.Data;
using BudgetTrackingNew.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BudgetTrackingNew.Pages;

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
                order by created desc limit 10
                """)
            .ToList();

        UserCategories = dbConnection
            .Query<Category>("""
                             select categories.id, name from categories
                                             left join user_categories on user_categories.category_id = categories.id
                                             where user_categories.user_id = '338e6139-b494-4c2f-86c5-d800e24f9058'
                             """
            ).ToList();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            LoadExpenses();
            return Page();
        }

        dbConnection.Execute(
            "INSERT INTO Expenses (Description, Amount, Created) VALUES (@Description, @Amount, @Created)",
            NewExpense);

        return RedirectToPage();
    }

    private void LoadExpenses()
    {
    }
}