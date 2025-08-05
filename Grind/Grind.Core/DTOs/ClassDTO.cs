using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Grind.Core.Enums;

namespace Grind.Core.DTOs
{
    public class ClassDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TrainerId { get; set; }
        public string TrainerUsername { get; set; }
        public int MaxCapacity { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public ClassCategory Category { get; set; }
        public int CurrentParticipants { get; set; }
        public bool IsCancelled { get; set; }
        public string? CancellationReason { get; set; }
    }

    public class ClassCreateDTO
    {
        [Required(ErrorMessage = "שם השיעור נדרש.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "שם השיעור חייב להיות בין 3 ל-100 תווים.")]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "שעת התחלה נדרשת.")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "שעת סיום נדרשת.")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "שם משתמש של מאמן נדרש.")]
        public string TrainerUsername { get; set; }

        [Required(ErrorMessage = "קיבולת מקסימלית נדרשת.")]
        [Range(1, 999, ErrorMessage = "קיבולת חייבת להיות מספר חיובי.")]
        public int MaxCapacity { get; set; }

        [Required(ErrorMessage = "רמת קושי נדרשת.")]
        public DifficultyLevel Difficulty { get; set; }

        [Required(ErrorMessage = "קטגוריית שיעור נדרשת.")]
        public ClassCategory Category { get; set; }
    }

    public class ClassRegistrationDTO
    {
        [Required]
        public int ClientId { get; set; }
        [Required]
        public int ClassId { get; set; }
    }

    public class ClassCancellationResultDTO
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class ClassRegistrationResultDTO
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public bool IsInWaitingList { get; set; } = false;
    }
}