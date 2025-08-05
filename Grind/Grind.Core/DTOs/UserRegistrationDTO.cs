// C:\Users\merav\OneDrive\שולחן העבודה\חדש לעבוד עליו\Grind\Grind.Core\DTOs\UserRegistrationDTO.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Grind.Core.Enums;
using Grind.Core.Entities; // ודא שזה קיים אם יש צורך בישויות ב-DTOs

namespace Grind.Core.DTOs
{
    public class AddressDTO
    {
        [Required(ErrorMessage = "רחוב נדרש.")]
        public string Street { get; set; }
        [Required(ErrorMessage = "עיר נדרשת.")]
        public string? City { get; set; }
        [Required(ErrorMessage = "מספר דירה נדרש.")]
        public string ApartmentNumber { get; set; }
    }

    public class PreferredTimeDTO
    {
        [Required(ErrorMessage = "יום מועדף נדרש.")]
        public DaysOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class PreferredClassDTO
    {
        [Required(ErrorMessage = "שם קטגוריית שיעור נדרש.")]
        public string ClassCategoryName { get; set; } // שינוי כאן: שם וסוג
    }

    public class UserRegistrationDTO
    {
        [Required(ErrorMessage = "שם משתמש נדרש.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "שם המשתמש חייב להיות בין 3 ל-50 תווים.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "סיסמה נדרשת.")]
        [MinLength(6, ErrorMessage = "הסיסמה חייבת להכיל לפחות 6 תווים.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "אימות סיסמה נדרש.")]
        [Compare("Password", ErrorMessage = "הסיסמאות אינן תואמות.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "כתובת דוא\"ל נדרשת.")]
        [EmailAddress(ErrorMessage = "פורמט הדוא\"ל אינו תקין.")]
        [StringLength(100, ErrorMessage = "כתובת הדוא\"ל ארוכה מדי.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "שם פרטי נדרש.")]
        [StringLength(50, ErrorMessage = "שם פרטי ארוך מדי. (מקסימום 50 תווים)")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "שם משפחה נדרש.")]
        [StringLength(50, ErrorMessage = "שם משפחה ארוך מדי.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "מספר טלפון נדרש.")]
        [Phone(ErrorMessage = "פורמט מספר הטלפון אינו תקין.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "תאריך לידה נדרש.")]
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1/1/1900", "1/1/2007", ErrorMessage = "תאריך לידה לא תקין או גיל צעיר מדי.")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "פרטי כתובת נדרשים.")]
        public AddressDTO Address { get; set; }

        [Required(ErrorMessage = "רמת קושי מועדפת נדרשת.")]
        public DifficultyLevel PreferredDifficulty { get; set; }

        [Required(ErrorMessage = "זמנים מועדפים נדרשים.")]
        public ICollection<PreferredTimeDTO> PreferredTimes { get; set; } = new List<PreferredTimeDTO>();

        [Required(ErrorMessage = "קטגוריות שיעור מועדפות נדרשות.")]
        public ICollection<PreferredClassDTO> PreferredClasses { get; set; } = new List<PreferredClassDTO>();
    }
}