using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Grind.Core.Entities;
using Grind.Core.Enums;

namespace Grind.Core.DTOs
{
    public class TrainerDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime HireDate { get; set; }
        public ICollection<ClassCategory> Specializations { get; set; }
        public AddressDTO Address { get; set; }
        public decimal? HourlyRate { get; set; }
    }

    public class TrainerRegistrationDTO
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

        [Required(ErrorMessage = "מספר טלפון נדרש.")] // הנחה שמספר טלפון הוא חובה למאמנים
        [Phone(ErrorMessage = "פורמט מספר הטלפון אינו תקין.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "תאריך לידה נדרש.")]
        [DataType(DataType.Date)]
        // טווח גילאים הגיוני למאמן. לדוגמה, לפחות בן 18. תאריך עליון - תלוי בלוגיקה עסקית שלך.
        [Range(typeof(DateTime), "1/1/1900", "1/1/2007", ErrorMessage = "תאריך לידה לא תקין. על המאמן להיות לפחות בן 18 (נכון לשנה זו).")]
        public DateTime DateOfBirth { get; set; }

        // --- שדות קשורים לכתובת ---
        // AddressDTO יכיל את פרטי הכתובת שהמאמן יזין.
        [Required(ErrorMessage = "פרטי כתובת נדרשים.")]
        public AddressDTO Address { get; set; }

        // --- שדות ספציפיים למאמן ---

        // Specializations - רשימת התמחויות שהמאמן מוסר
        [Required(ErrorMessage = "יש לציין לפחות התמחות אחת.")]
        [MinLength(1, ErrorMessage = "יש לציין לפחות התמחות אחת.")]
        public ICollection<ClassCategory> Specializations { get; set; } = new List<ClassCategory>();

        
    }

    public class TrainerSalaryDTO
    {
        public int TrainerId { get; set; }
        public decimal MonthlySalary { get; set; }
        public int ClassesTaughtThisMonth { get; set; }
    }
    public class TrainerProfileUpdateDTO
    {
        // פרטים אישיים ניתנים לעדכון
        [StringLength(50, ErrorMessage = "שם פרטי ארוך מדי.")]
        public string FirstName { get; set; }

        [StringLength(50, ErrorMessage = "שם משפחה ארוך מדי.")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "פורמט הדוא\"ל אינו תקין.")]
        [StringLength(100, ErrorMessage = "כתובת הדוא\"ל ארוכה מדי.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "פורמט מספר הטלפון אינו תקין.")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        // פרטי כתובת ניתנים לעדכון (אם ה-AddressDTO הוא חלק מהפרופיל)
        public AddressDTO Address { get; set; }

        // שדות ספציפיים למאמן
        public ICollection<ClassCategory> Specializations { get; set; } // יכול להיות null אם אין שינוי
        public decimal? HourlyRate { get; set; } // אם ניתן לעדכון (לרוב על ידי אדמין)
        public DateTime HireDate { get; set; }

    }
}