using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grind.Core.DTOs;

namespace Grind.Core.Interfaces
{
    public interface IUserService
    {
        //Task<bool> RegisterClientAsync(UserRegistrationDTO registrationDto); // זה כבר ממומש ב-IClientService
        Task<LoginResponseDTO> LoginAsync(UserLoginDTO loginDto);
        Task<bool> UserExistsAsync(string username); // לבדיקה לפני רישום

        // חדש: מתודה ליצירת טוקן
        string GenerateJwtToken(string userId, string username, string userRole);
    }
}