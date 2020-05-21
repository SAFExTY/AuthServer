using System.ComponentModel.DataAnnotations;

namespace AuthServer.Models
{
    public class AuthenticateModel
    {
        [Required] public string Username { get; set; }

        [Required] public string Password { get; set; }
    }

    public class CreateModel
    {
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required] public string Username { get; set; }
        [Required] public string Email { get; set; }
        [Required] public string Password { get; set; }
    }
}