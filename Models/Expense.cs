namespace BudgetTrackingNew.Models;

public class Expense
{
    public int Id { get; set; }
    public string Description { get; set; }
    public double Amount { get; set; }
    public DateTime Created { get; set; }
    public DateTime Inputted { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public Guid UserId { get; set; }
}
