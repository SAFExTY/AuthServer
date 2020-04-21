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
                        new Claim(ClaimTypes.Name, user.Id.ToString()),
                    }),
                    // todo remove magic expiration value
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                user.Token = tokenHandler.WriteToken(token);
                return user;
            }
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