using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grind.Core.DTOs;
using Grind.Core.Entities; // עבור Trainer entity אם מחזירים אותו

namespace Grind.Core.Interfaces
{
    public interface ITrainerService
    {
        Task<TrainerSalaryDTO> GetTrainerMonthlySalaryAsync(int trainerId);
        Task<string> AddTrainerAsync(TrainerRegistrationDTO trainerDto);
        Task<IEnumerable<TrainerDTO>> GetAllTrainersAsync();
        public Task<TrainerDTO?> UpdateTrainerProfileAsync(int trainerId, TrainerProfileUpdateDTO dto);
        Task<TrainerDTO> GetTrainerProfileAsync(int trainerId);
        Task<string> TerminateTrainerAsync(int trainerId);
        // Task<TrainerDTO> GetTrainerByIdAsync(int trainerId); // אולי
    }
}