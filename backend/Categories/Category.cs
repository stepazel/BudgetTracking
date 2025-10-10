using backend.Users;

namespace backend.Categories;

public class Category
{
    public int Id { get; private set; }
    
    public string Name { get; private set; }
    
    public ICollection<User> Users { get; private set; }

    public Category(string name)
    {
        Name = name;
    }
}