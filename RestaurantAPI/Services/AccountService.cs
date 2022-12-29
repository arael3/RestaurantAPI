using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestaurantAPI.Entities;
using RestaurantAPI.Exceptions;
using RestaurantAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RestaurantAPI.Services
{
    public interface IAccountService
    {
        string GenerateJwt(LoginDto dto);
        void Register(RegisterUserDto dto);
    }

    public class AccountService : IAccountService
    {
        private readonly RestaurantDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly AuthenticationSettings _authenticationSettings;

        public AccountService(RestaurantDbContext dbContext, IMapper mapper, IPasswordHasher<User> passwordHasher, AuthenticationSettings authenticationSettings)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _authenticationSettings = authenticationSettings;
        }

        public string GenerateJwt(LoginDto dto)
        {
            var user = _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == dto.Email);

            if (user is null)
            {
                throw new BadRequestException("Invalid username or password");
            }

            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                throw new BadRequestException("Invalid username or password");
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, $"{user.Role.Name}"),
                new Claim("DateOfBirth", user.DateOfBirth.Value.ToString("yyyy-MM-dd"))
            };

            if (!string.IsNullOrEmpty(user.Nationality))
            {
                claims.Add(new Claim("Nationality", user.Nationality));
            }

            // utworzenie klucza prywatnego
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.JwtKey));

            // wygenerowanie list uwierzytelniających (credentials) potrzebnych do podpisania tokenu JWT
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // ustalnie daty ważności tokenu na podstawie liczby dni określonej w appsettings.json >> Authentication >> JwtExpireDays
            // nie odnosimy się bezpośrednio do appsettings.json tylko do obiektu sinleton _authenticationSettings, który został powiązany z sekcją Authentication w pliku appsettings.json. Powiązanie to jest zdefiniowane w pliku Program.cs
            var expires = DateTime.Now.AddDays(_authenticationSettings.JwtExpireDays);

            // generowanie tokenu JWT
            var token = new JwtSecurityToken(
                _authenticationSettings.JwtIssuer,
                _authenticationSettings.JwtIssuer,
                claims,
                expires: expires,
                signingCredentials: cred);

            // wygenerowanie tokenu JWT w formie string
            var tokenHandler = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenHandler;
        }

        public void Register(RegisterUserDto dto)
        {
            //var user = _mapper.Map<User>(dto);
            var user = new User
            {
                Email = dto.Email,
                DateOfBirth = dto.DateOfBirth,
                Nationality = dto.Nationality,
                RoleId = dto.RoleId
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }
    }

   
}
