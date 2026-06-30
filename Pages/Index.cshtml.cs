using System.Data;
using BudgetTrackingNew.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BudgetTrackingNew.Pages;

public class IndexModel(IDbConnection dbConnection) : PageModel
{
    public List<Expense> Expenses { get; set; }

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
    }

    public void OnPost()
    {
    }
}