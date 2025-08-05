using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grind.Core.Enums;

namespace Grind.Core.Entities
{
    public class Client : Person
    {
        public decimal MonthlyPaymentAmount { get; set; } // כמה משלם בחודש
        public decimal BalanceDue { get; set; } // יתרה לתשלום

        // העדפות הלקוח:
        public DifficultyLevel PreferredDifficulty { get; set; }
        public ICollection<ClientPreferredTime> ClientPreferredTimes { get; set; } = new List<ClientPreferredTime>(); // ימים מועדפים
        public ICollection<ClientPreferredClass> ClientPreferredClasses { get; set; } = new List<ClientPreferredClass>(); // קטגוריות מועדפות

        // קשרי גומלין:
        public ICollection<ClientClass> ClientClasses { get; set; } = new List<ClientClass>(); // שיעורים אליהם רשום
        public ICollection<WaitingListEntry> WaitingListEntries { get; set; } = new List<WaitingListEntry>(); // רשימות המתנה
        public ICollection<Payment> Payments { get; set; } = new List<Payment>(); // תשלומים שבוצעו
    }
}