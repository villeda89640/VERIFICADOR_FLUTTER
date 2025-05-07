using SapApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace SapApi.Services
{
    public static class FakeUserRepository
    {
        private static List<User> _users = new()
        {
            new User { Username = "admin", Password = "123", Role = "Admin" },
            new User { Username = "user", Password = "123", Role = "User" }
        };

        public static User GetUser(string username, string password)
        {
            return _users.FirstOrDefault(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);
        }
    }
}
