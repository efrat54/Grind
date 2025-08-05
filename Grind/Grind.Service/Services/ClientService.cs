using AutoMapper;
using Grind.Core.DTOs;
using Grind.Core.Entities;
using Grind.Core.Interfaces;
using Grind.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grind.Core.Enums;
using BCrypt.Net;

namespace Grind.Service.Services
{
    public class ClientService : IClientService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public ClientService(DataContext context, IMapper mapper, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
        }
        //----
        public async Task<string> RegisterClientAsync(UserRegistrationDTO registrationDto)
        {
            if (await _context.People.AnyAsync(p => p.Username == registrationDto.Username))
            {
                return "שם משתמש זה כבר קיים.";
            }
            if (await _context.People.AnyAsync(p => p.Email == registrationDto.Email))
            {
                return "כתובת דוא\"ל זו כבר רשומה.";
            }

            var client = _mapper.Map<Client>(registrationDto);
            client.HashedPassword = BCrypt.Net.BCrypt.HashPassword(registrationDto.Password);
            client.MonthlyPaymentAmount = 100;//לבדוק אם לא נצרך
            client.BalanceDue = 100;//לבדוק אם מבטלת
            client.IsActive = true;
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return null;
        }
        //---
        public async Task<ClientDTO> GetClientProfileAsync(int clientId)
        {
            var client = await _context.Clients
                .Include(c => c.ClientPreferredTimes)
                .Include(c => c.ClientPreferredClasses)
                .Include(c=>c.Address)
                .FirstOrDefaultAsync(c => c.Id == clientId);
            return _mapper.Map<ClientDTO>(client);
        }
//כרגע לא בשימוש
        public async Task<IEnumerable<ClientDTO>> GetAllClientsAsync()
        {
            var clients = await _context.Clients
                                        .Include(c => c.ClientPreferredTimes)
                                        .Include(c => c.ClientPreferredClasses)
                                        .ToListAsync();
            return _mapper.Map<IEnumerable<ClientDTO>>(clients);
        }

        public async Task<ClientDTO?> UpdateClientProfileAsync(int clientId, ClientProfileUpdateDTO dto)
        {
            var client = await _context.Clients.Include(c => c.Address).Include(c => c.ClientPreferredTimes).Include(c => c.ClientPreferredClasses).FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null)
                return null;

            client.FirstName = dto.FirstName;
            client.LastName = dto.LastName;
            client.Email = dto.Email;
            client.PhoneNumber = dto.PhoneNumber;
            client.DateOfBirth = dto.DateOfBirth;
            client.PreferredDifficulty = dto.PreferredDifficulty;

            client.ClientPreferredTimes = dto.PreferredDays?
                .Select(day => new ClientPreferredTime { ClientId = client.Id, PreferredDay = day })
                .ToList() ?? new List<ClientPreferredTime>();

            client.ClientPreferredClasses = dto.PreferredCategories?
                .Select(cat => new ClientPreferredClass { ClientId = client.Id, ClassCategory = Enum.Parse<ClassCategory>(cat) })
                .ToList() ?? new List<ClientPreferredClass>();

            if (dto.Address != null)
            {
                if (client.Address == null)
                    client.Address = new Address();

                client.Address.Street = dto.Address.Street;
                client.Address.City = dto.Address.City;
                client.Address.ApartmentNumber = dto.Address.ApartmentNumber;
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<ClientDTO>(client);
        }
        //כרגע לא בשימוש
        public async Task<ClientPaymentStatusDTO> GetClientPaymentStatusAsync(int clientId)
        {
            var client = await _context.Clients
                .Include(c => c.Payments)
                .FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null) return null;

            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;

            var totalPaidThisMonth = client.Payments
                .Where(p => p.PaymentDate.Month == currentMonth && p.PaymentDate.Year == currentYear)
                .Sum(p => p.Amount);

            var balanceDue = client.MonthlyPaymentAmount - totalPaidThisMonth;

            return new ClientPaymentStatusDTO
            {
                ClientId = clientId,
                MonthlyPaymentDue = client.MonthlyPaymentAmount,
                TotalPaidThisMonth = totalPaidThisMonth,
                BalanceDue = balanceDue,
                LastPaymentDate = client.Payments.OrderByDescending(p => p.PaymentDate).FirstOrDefault()?.PaymentDate ?? DateTime.MinValue,
                IsSubscriptionActive = client.IsActive
            };
        }

        //כרגע לא בשימוש
        public async Task<bool> CancelSubscriptionAsync(int clientId)
        {
            var client = await _context.Clients
                .Include(c => c.ClientClasses)
                    .ThenInclude(cc => cc.Class)
                .FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null)
            {
                return false;
            }

            if (!client.IsActive)
            {
                return false;
            }

            client.IsActive = false;
            client.BalanceDue = 0;

            var futureClassRegistrations = client.ClientClasses
                .Where(cc => cc.Class != null && cc.Class.StartTime > DateTime.UtcNow)
                .ToList();

            if (futureClassRegistrations.Any())
            {
                _context.ClientClasses.RemoveRange(futureClassRegistrations);

                foreach (var classRegistration in futureClassRegistrations)
                {
                    var classEntity = await _context.Classes.FindAsync(classRegistration.ClassId);
                    if (classEntity != null && classEntity.CurrentParticipants > 0)
                    {
                        classEntity.CurrentParticipants--;

                        var nextInWaitingList = await _context.WaitingListEntries
                            .Where(wl => wl.ClassId == classEntity.Id)
                            .OrderBy(wl => wl.JoinDate)
                            .FirstOrDefaultAsync();

                        if (nextInWaitingList != null)
                        {
                            var newClientClass = new ClientClass { ClientId = nextInWaitingList.ClientId, ClassId = classEntity.Id };
                            _context.ClientClasses.Add(newClientClass);
                            classEntity.CurrentParticipants++;
                            _context.WaitingListEntries.Remove(nextInWaitingList);

                            var clientOnWaitingList = await _context.Clients.FindAsync(nextInWaitingList.ClientId);
                            if (clientOnWaitingList != null)
                            {
                                await _emailService.SendEmailAsync(clientOnWaitingList.Email,
                                        "מקום התפנה בשיעור!",
                                        $"שלום {clientOnWaitingList.FirstName},\nמקום התפנה בשיעור {classEntity.Name} בתאריך {classEntity.StartTime.ToShortDateString()} בשעה {classEntity.StartTime.ToShortTimeString()}.");
                            }
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendEmailAsync(
                    client.Email,
                    "אישור ביטול מנוי Grind",
                    $"שלום {client.FirstName},\nאנו מאשרים את ביטול המנוי שלך למכון Grind. כל הרישומים שלך לשיעורים עתידיים בוטלו.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending cancellation email to {client.Email}: {ex.Message}");
            }

            return true;
        }
    }
} 