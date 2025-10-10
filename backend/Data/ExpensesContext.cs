using backend.Categories;
using backend.Expenses;
using backend.Users;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class ExpensesContext : DbContext
{
    public ExpensesContext(DbContextOptions<ExpensesContext> options) : base(options)
    {
    }

    public DbSet<Expense> Expenses { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Category> Categories { get; set; }
}