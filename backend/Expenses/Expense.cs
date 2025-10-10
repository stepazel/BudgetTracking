using backend.Categories;
using backend.Users;

namespace backend.Expenses;

public class Expense
{
    public int Id { get; private set; }
    public string Description { get; private set; }

    public double Amount { get; private set; }

    public DateTime Created { get; private set; }

    public DateTime Inputted { get; private set; }

    public User User { get; private set; }

    public Category Category { get; private set; }

    public Expense(string description, double amount, DateTime created, DateTime inputted)
    {
        Description = description;
        Amount = amount;
        Created = created;
        Inputted = inputted;
    }
}