using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grind.Core.DTOs;

namespace Grind.Core.Interfaces
{
    public interface IClientService
    {
        Task<string> RegisterClientAsync(UserRegistrationDTO registrationDto);
        Task<ClientDTO> GetClientProfileAsync(int clientId);
        Task<ClientDTO> UpdateClientProfileAsync(int clientId, ClientProfileUpdateDTO profileDto); // 💡 שונה מ-bool ל-ClientDTO
        Task<ClientPaymentStatusDTO> GetClientPaymentStatusAsync(int clientId);
        Task<bool> CancelSubscriptionAsync(int clientId);
        Task<IEnumerable<ClientDTO>> GetAllClientsAsync(); // 👈 חדש: חתימת מתודה לקבלת כל הלקוחות
    }
}