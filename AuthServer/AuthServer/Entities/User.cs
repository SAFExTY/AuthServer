namespace AuthServer.Entities
{
    public interface IUser
    {
        public string GameId { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Username { get; }
        public string Email { get; }
        public string Token { get; set; }
    }

    public class User : IUser
    {
        public string GameId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        
    }

    public sealed class InternalUser : User
    {
        public string Password { get; set; }
    }
}