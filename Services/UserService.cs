using TestWebApp.Models;
namespace TestWebApp.Services;

public class UserService
{
    private readonly List<User> _users = new();
    public UserService()
    {
        _users.Add(new User
        {
            Login = "admin",
            Password = "admin",
            Name = "Admin",
            Gender = 1,
            Admin = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        });
    } 
    public IEnumerable<User> GetAll() => _users;
    public User? GetByLogin(string login) => _users.FirstOrDefault(u => u.Login == login);
    public void Add(User user) => _users.Add(user);
    public void Update(User user)
    {
        var index = _users.FindIndex(u => u.Id == user.Id);
        if (index >= 0) _users[index] = user;
    }
    public bool LoginExists(string login) => _users.Any(u => u.Login == login);
    
    public bool CanEdit(string whoEditByLogin, User targetUser)
    {
        if (targetUser.RevokedOn != null) return false;
        if (targetUser.Login == whoEditByLogin) return true;

        var currentUser = GetByLogin(whoEditByLogin);
        return currentUser is { Admin: true };
    }

    public void Remove(User user)
    {
        _users.Remove(user);
    }
}