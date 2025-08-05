using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grind.Core.DTOs;
using Grind.Core.Entities;

namespace Grind.Core.Interfaces
{
    public interface IClassService
    {
        Task<IEnumerable<ClassDTO>> GetAllClassesAsync();
        Task<IEnumerable<ClassDTO>> GetClientClassesAsync(int clientId);
        
        Task<ClassDTO> GetClassByIdAsync(int classId); // 👈 חדש
        Task<ClassRegistrationResultDTO> RegisterClientForClassAsync(int clientId, int classId);
        Task<bool> CancelClientClassRegistrationAsync(int clientId, int classId);
        public Task<bool> AddClassAsync(ClassCreateDTO classCreateDto);
        Task<bool> CancelClassAsync(int classId);
        Task<bool> UpdateClassAsync(int classId, ClassDTO classDto);
        Task<bool> AddClientToWaitingListAsync(int clientId, int classId);
        Task<IEnumerable<ClassDTO>> GetClassesByTrainerUsernameAsync(string trainerUsername);

    }
}