using System;
using System.Collections.Generic;
using System.Linq; 
using System.Text;
using System.Threading.Tasks;
using AutoMapper; 
using Grind.Core.DTOs; 
using Grind.Core.Entities;
using Grind.Core.Interfaces; 
using Grind.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; 
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims; 

namespace Grind.Service.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserService(DataContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration; 
        }

        public async Task<LoginResponseDTO> LoginAsync(UserLoginDTO loginDto)
        {
            var user = await _context.People.FirstOrDefaultAsync(p => p.Username == loginDto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.HashedPassword))
            {
                return null; 
            }
            string userRole = "Unknown";
            if (user is Client)
            {
                userRole = "Client";
            }
            else if (user is Trainer)
            {
                userRole = "Trainer";
            }
            else if (user is Admin)
            {
                userRole = "Admin";
            }

            var token = GenerateJwtToken(user.Id.ToString(), user.Username, userRole);

            return new LoginResponseDTO
            {
                Token = token,
                RefreshToken = null, 
                Role = userRole,
                UserId = user.Id
            };
        }


        public string GenerateJwtToken(string userId, string username, string userRole)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId), 
                new Claim(ClaimTypes.Name, username), 
                new Claim(ClaimTypes.Role, userRole) 
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.People.AnyAsync(p => p.Username == username);
        }
    }
}