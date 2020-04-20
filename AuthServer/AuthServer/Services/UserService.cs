using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using AuthServer.Entities;
using AuthServer.Helpers;
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
        // todo store user in DB with hashed password
        private ISet<IUser> _users = new HashSet<IUser>();
        private PasswordHasher<InternalUser> passwordHasher = new PasswordHasher<InternalUser>();

        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            var defaultUser = new InternalUser
            {
                Id = 1, FirstName = "Valentin", LastName = "Chassignol", Username = "Tester"
            };
            defaultUser.Password = passwordHasher.HashPassword(defaultUser, "saucisse");
            _users.Add(defaultUser);
        }

        public IUser Authenticate(string username, string password)
        {
            // Username can also be the email
            var user = (InternalUser) _users.SingleOrDefault(u =>
                u.Username.ToLowerInvariant().Equals(username.ToLowerInvariant()) ||
                u.Email.ToLowerInvariant().Equals(username.ToLowerInvariant()));

            // User not found
            if (user == null)
                return null;

            var result = passwordHasher.VerifyHashedPassword(user, user.Password, password);
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

        public IEnumerable<IUser> GetAll()
        {
            return _users;
        }
    }
}