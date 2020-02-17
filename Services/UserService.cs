using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using auth.Business;
using auth.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace auth.Services
{
    public interface IUserService
    {
        IEnumerable<User> GetAll();
        User Authenticate(string modelUsername, string modelPassword);
    }
    
    public class UserService : IUserService
    {
        private IList<User> _users = new List<User>
        {
            new User { Id = 1, FirstName = "Arnaldo", LastName = "Antunes", Username = "arnaldo", Password = "antunes" },
            new User { Id = 2, FirstName = "Carlinhos", LastName = "Brown", Username = "carlinhos", Password = "brown" },
            new User { Id = 3, FirstName = "Marisa", LastName = "Monte", Username = "marisa", Password = "monte" }
        };

        private readonly AppSettings _appSettings;
        
        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public IEnumerable<User> GetAll()
        {
            return _users.WithoutPasswords();
        }
        
        public User Authenticate(string modelUsername, string modelPassword)
        {
            var user = _users.FirstOrDefault(x => x.Username == modelUsername && x.Password == modelPassword);

            if (user == null) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            return user.WithoutPassword();
        }
    }
}