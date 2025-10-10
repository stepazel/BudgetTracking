using backend.Categories;
using backend.Expenses;

namespace backend.Users;

public class User
{
    public User(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public int Id { get; private set; }

    public string Username { get; private set; }
    
    public string Password { get; private set; }
    
    public ICollection<Expense> Expenses { get; private set; }
    
    public ICollection<Category> Categories { get; private set; }
}