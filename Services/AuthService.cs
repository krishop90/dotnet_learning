using BooksApi.DTOs;
using BooksApi.Entities;
using BooksApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BooksApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly BookDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(BookDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<SignInResponse> SignInAsync(SignInRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email &&
                                        u.Password == request.Password);

            if (user == null)
                return null;

            var token = GenerateJwtToken(user);

            return new SignInResponse
            {
                AccessToken = token,
                TokenType = "Bearer"
            };
        }

        public async Task<SignInResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return null;

            var user = new User
            {
                Email = request.Email,
                Password = request.Password,
                Role = UserRole.User 
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return new SignInResponse
            {
                AccessToken = token,
                TokenType = "Bearer"
            };
        }

        public async Task<IEnumerable<UserModel>> GetUsersAsync(string? roleFilter = "user")
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(roleFilter))
            {
                query = query.Where(u => u.Role == roleFilter);
            }

            var users = await query
                .Select(u => new UserModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();

            return users;
        }

        public async Task<UserListResponse> GetUsersAsync(
            string? roleFilter = "user",
            string search = "",
            string sort = "asc",
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(roleFilter))
            {
                query = query.Where(u => u.Role == roleFilter);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(u => u.Email.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();

            query = sort.ToLower() == "desc" 
                ? query.OrderByDescending(u => u.Id)
                : query.OrderBy(u => u.Id);

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();

            return new UserListResponse
            {
                Users = users,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found")));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}