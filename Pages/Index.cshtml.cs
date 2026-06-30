using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BudgetTrackingNew.Pages;

public class IndexModel(IDbConnection dbConnection) : PageModel
{
    public void OnGet()
    {
        var expenses = dbConnection.Query("SELECT * FROM Expenses");
        Console.WriteLine(expenses.FirstOrDefault());
        
    }
    
    public void OnPost()
    {
    }
}