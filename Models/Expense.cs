using System.ComponentModel.DataAnnotations;

namespace BudgetTrackingNew.Models;

public class Expense
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Popis výdaje je povinný")]
    public string Description { get; set; } = string.Empty;
    
    [Range(0.01, 1000000, ErrorMessage = "Částka musí být kladná")]
    public double Amount { get; set; }
    public DateTime Created { get; set; }
    public DateTime Inputted { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public Guid UserId { get; set; }
}
