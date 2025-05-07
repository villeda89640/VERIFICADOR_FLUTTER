namespace SapApi.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }  // Se recomienda hashear
        public string Role { get; set; }      // Ej: "Admin", "User"
    }
}
