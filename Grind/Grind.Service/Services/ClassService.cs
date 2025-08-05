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
using Grind.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace Grind.Service.Services
{
    public class ClassService : IClassService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClassService(DataContext context, IMapper mapper, IEmailService emailService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }
//----
        public async Task<IEnumerable<ClassDTO>> GetAllClassesAsync()
        {
            var classes = await _context.Classes
                .Include(c => c.Trainer)
                .Where(c => !c.IsCancelled)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ClassDTO>>(classes);
        }
//-----
        public async Task<IEnumerable<ClassDTO>> GetClientClassesAsync(int clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null || !client.IsActive)
            {
                return null;
            }

            var classes = await _context.ClientClasses
                .Where(cc => cc.ClientId == clientId)
                .Include(cc => cc.Class) 
                .ThenInclude(c => c.Trainer) 
                .Select(cc => cc.Class) 
                .Where(c => !c.IsCancelled)
                .ToListAsync();
            return _mapper.Map<IEnumerable<ClassDTO>>(classes);
        }
        //כרגע לא בשימוש
        public async Task<ClassDTO> GetClassByIdAsync(int classId)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Trainer)
                .FirstOrDefaultAsync(c => c.Id == classId);
            return _mapper.Map<ClassDTO>(classEntity);
        }
        //לבדוק רשימת המתנה
        public async Task<ClassRegistrationResultDTO> RegisterClientForClassAsync(int clientId, int classId)
        {
            var classEntity = await _context.Classes.FindAsync(classId);
            var client = await _context.Clients.FindAsync(clientId);

            if (classEntity == null || client == null)
            {
                return new ClassRegistrationResultDTO { IsSuccess = false, Message = "Class or Client not found." };
            }

            if (!client.IsActive)
            {
                return new ClassRegistrationResultDTO { IsSuccess = false, Message = "Client subscription is not active. Cannot register for classes." };
            }

            var alreadyRegistered = await _context.ClientClasses
                .AnyAsync(cc => cc.ClientId == clientId && cc.ClassId == classId);
            if (alreadyRegistered)
            {
                return new ClassRegistrationResultDTO { IsSuccess = false, Message = "Client already registered for this class." };
            }

            if (classEntity.CurrentParticipants < classEntity.MaxCapacity)
            {
                var clientClass = new ClientClass { ClientId = clientId, ClassId = classId };
                _context.ClientClasses.Add(clientClass);
                classEntity.CurrentParticipants++;
                await _context.SaveChangesAsync();
                return new ClassRegistrationResultDTO { IsSuccess = true, Message = "Successfully registered for class." };
            }
            else
            {
                var waitingListEntry = new WaitingListEntry
                {
                    ClientId = clientId,
                    ClassId = classId,
                    JoinDate = DateTime.UtcNow,
                    Position = await _context.WaitingListEntries.CountAsync(wl => wl.ClassId == classId) + 1
                };
                _context.WaitingListEntries.Add(waitingListEntry);
                await _context.SaveChangesAsync();
                return new ClassRegistrationResultDTO { IsSuccess = true, Message = "Successfully added to waiting list.", IsInWaitingList = true };
            }
        }
        //----
        public async Task<bool> CancelClientClassRegistrationAsync(int clientId, int classId)
        {
            var clientClass = await _context.ClientClasses
                .FirstOrDefaultAsync(cc => cc.ClientId == clientId && cc.ClassId == classId);

            if (clientClass == null) return false;

            _context.ClientClasses.Remove(clientClass);

            var classEntity = await _context.Classes.FindAsync(classId);
            if (classEntity != null)
            {
                classEntity.CurrentParticipants--;

                var nextInWaitingList = await _context.WaitingListEntries
                    .Where(wl => wl.ClassId == classId)
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
            await _context.SaveChangesAsync();
            return true;
        }
        //---
        public async Task<bool> AddClassAsync(ClassCreateDTO classCreateDto)
        {
            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.Username == classCreateDto.TrainerUsername);

            if (trainer == null)
            {
                return false;
            }

            var classEntity = _mapper.Map<Class>(classCreateDto);

            classEntity.TrainerId = trainer.Id;
            classEntity.CurrentParticipants = 0;
            classEntity.IsCancelled = false;

            _context.Classes.Add(classEntity);
            await _context.SaveChangesAsync();
            //שליחת התראות ללקוחות שהשיעור בעדיפות שלהם
            //var clientsToNotify = await _context.Clients
            //    .Include(c => c.ClientPreferredTimes)
            //    .Include(c => c.ClientPreferredClasses)
            //    .Where(c => c.PreferredDifficulty == classEntity.Difficulty &&
            //                c.ClientPreferredTimes.Any(cpt => cpt.PreferredDay == (Grind.Core.Enums.DaysOfWeek)classEntity.StartTime.DayOfWeek) &&
            //                c.ClientPreferredClasses.Any(cpc => cpc.ClassCategory == classEntity.Category))
            //    .ToListAsync();

            //foreach (var client in clientsToNotify)
            //{
            //    await _emailService.SendEmailAsync(client.Email,
            //        "שיעור חדש שעשוי לעניין אותך!",
            //        $"שלום {client.FirstName},\nהוסף שיעור חדש בשם {classEntity.Name} בתאריך {classEntity.StartTime.ToShortDateString()} בשעה {classEntity.StartTime.ToShortTimeString()}. רמת קושי: {classEntity.Difficulty}, קטגוריה: {classEntity.Category}.");
            //}
            return true;
        }
        //---
        public async Task<bool> CancelClassAsync(int classId)
        {
            var classToCancel = await _context.Classes
                .Include(c => c.ClientClasses)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classToCancel == null) return false;

            classToCancel.IsCancelled = true;
            await _context.SaveChangesAsync();
            //הודעה ללקוחות ששיעור בוטל
            //foreach (var clientClass in classToCancel.ClientClasses)
            //{
            //    var client = await _context.Clients.FindAsync(clientClass.ClientId);
            //    if (client != null)
            //    {
            //        await _emailService.SendEmailAsync(client.Email,
            //            "ביטול שיעור!",
            //            $"שלום {client.FirstName},\nהשיעור {classToCancel.Name} בתאריך {classToCancel.StartTime.ToShortDateString()} בשעה {classToCancel.StartTime.ToShortTimeString()} בוטל. אנו מתנצלים על אי הנוחות.");
            //    }
            //}
            return true;
        }
        //---
        public async Task<bool> UpdateClassAsync(int classId, ClassDTO classDto)
        {
            var classToUpdate = await _context.Classes.Include(c => c.Trainer).FirstOrDefaultAsync(c => c.Id == classId);

            if (classToUpdate == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(classDto.TrainerUsername) && (classToUpdate.Trainer == null || classToUpdate.Trainer.Username != classDto.TrainerUsername))
            {
                var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.Username == classDto.TrainerUsername);
                if (trainer == null)
                {
                    return false;
                }
                classToUpdate.TrainerId = trainer.Id;
            }

            _mapper.Map(classDto, classToUpdate);

            if (!await _context.Trainers.AnyAsync(t => t.Id == classToUpdate.TrainerId))
            {
                return false;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
        //לבדוק
        public async Task<bool> AddClientToWaitingListAsync(int clientId, int classId)
        {
            var waitingListEntry = new WaitingListEntry
            {
                ClientId = clientId,
                ClassId = classId,
                JoinDate = DateTime.UtcNow,
                Position = await _context.WaitingListEntries.CountAsync(wl => wl.ClassId == classId) + 1
            };
            _context.WaitingListEntries.Add(waitingListEntry);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<ClassDTO>> GetClassesByTrainerUsernameAsync(string trainerUsername)
        {
            var classes = await _context.Classes.Include(c => c.Trainer).Where(c => !c.IsCancelled && c.Trainer != null && c.Trainer.Username == trainerUsername)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ClassDTO>>(classes);
        }

    }
}