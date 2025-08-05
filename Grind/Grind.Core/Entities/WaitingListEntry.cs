using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Grind.Core.Entities
{
    public class WaitingListEntry
    {
        public int Id { get; set; } // מפתח ראשי
        public int ClientId { get; set; }
        public Client Client { get; set; }

        public int ClassId { get; set; }
        public Class Class { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public int Position { get; set; } // מקומו בתור ברשימת ההמתנה
    }
}