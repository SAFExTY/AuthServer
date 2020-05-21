using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using AuthServer.Entities;
using AuthServer.Helpers;
using AuthServer.Migrations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Services
{
    public interface IUserService
    {
        IUser Authenticate(string username, string password);
        bool Exist(string username, string email);
        IUser Create(InternalUser user);
        IEnumerable<IUser> GetAll();
    }

    public class UserService : IUserService
    {
        // hardcoded users database
        private readonly PasswordHasher<IUser> _passwordHasher = new PasswordHasher<IUser>();

        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public IUser Authenticate(string username, string password)
        {
            // Username can also be the email
            var userTask = DatabaseManager.GetDatabaseManager().GetUser(username);
            lock (userTask)
            {
                // User not found
                var user = userTask.Result;
                if (user == null)
                    return null;

                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
                if (result == PasswordVerificationResult.Failed)
                    return null;

                // Authentication successful, generate jwt token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.GameId),
                    }),
                    // todo remove magic expiration value
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                // Prevent to send sensitive information like the password
                return new User
                {
                    Email = user.Email,
                    GameId = user.GameId,
                    FirstName = user.FirstName,
                    Username = user.Username,
                    LastName = user.LastName,
                    Token = tokenHandler.WriteToken(token),
                };
            }
        }

        public bool Exist(string username, string email)
        {
            var task = DatabaseManager.GetDatabaseManager().GetUser(username, email);
            lock (task)
            {
                return task.Result != null;
            }
        }

        public IUser Create(InternalUser user)
        {
            var adb = DatabaseManager.GetDatabaseManager().OpenConnection("sshcity", true);
            var rawPassword = user.Password;
            user.Password = _passwordHasher.HashPassword(user, rawPassword); // Replace password with hashed version
            user.GameId = Guid.NewGuid().ToString(); // Generate a new GameID
            user.Token = null; // Ensure null
            adb.Document.PostDocumentAsync(
                "users",
                user
            );
            return Authenticate(user.Username, rawPassword);
        }

        public IEnumerable<IUser> GetAll()
        {
            var users = DatabaseManager.GetDatabaseManager().GetUsers();
            lock (users)
            {
                return users.Result.Select(u => (User) u);
            }
        }
    }
}