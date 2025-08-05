using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grind.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Grind.Core.DTOs
{
    public class ClientDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public AddressDTO Address { get; set; }
        public DifficultyLevel PreferredDifficulty { get; set; }
        public List<DaysOfWeek> PreferredDays { get; set; } = new List<DaysOfWeek>();
        public List<string> PreferredCategories { get; set; } = new List<string>(); // אם הקטגוריות הן מחרוזות או Enum אחר
        public bool IsActive { get; set; }

    }

    public class ClientProfileUpdateDTO
    {
        // שדות כלליים הניתנים לעדכון (מיורשים מ-Person)
        public string Username { get; set; } // בדרך כלל לא ניתן לעדכן שם משתמש, אבל אם כן אז להוסיף.
                                             // אם אסור, נסיר מפה ומה-Frontend. נשאיר כרגע.
        [StringLength(50, ErrorMessage = "שם פרטי ארוך מדי.")]
        public string FirstName { get; set; }

        [StringLength(50, ErrorMessage = "שם משפחה ארוך מדי.")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "פורמט הדוא\"ל אינו תקין.")]
        [StringLength(100, ErrorMessage = "כתובת הדוא\"ל ארוכה מדי.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "פורמט מספר הטלפון אינו תקין.")]
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
         public AddressDTO Address { get; set; } // אם יש AddressDTO ואתם רוצים לאפשר עדכון כתובת

        // שדות ספציפיים ללקוח
        public decimal MonthlyPaymentAmount { get; set; } // הלקוח לא עורך זאת ב-frontend, אבל אולי ה-admin כן
        public DifficultyLevel PreferredDifficulty { get; set; }
        public List<DaysOfWeek> PreferredDays { get; set; } = new List<DaysOfWeek>();
        public List<string> PreferredCategories { get; set; } = new List<string>();

        // ה-BalanceDue לא צריך להיות כאן כי הוא מחושב ולא ניתן לעריכה ע"י המשתמש או ה-admin
    }
    public class ClientPaymentStatusDTO
    {
        public int ClientId { get; set; }
        public decimal MonthlyPaymentDue { get; set; }
        public decimal TotalPaidThisMonth { get; set; }
        public decimal BalanceDue { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public bool IsSubscriptionActive { get; set; }

    }
}