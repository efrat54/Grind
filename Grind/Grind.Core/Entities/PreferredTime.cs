using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grind.Core.Enums;

namespace Grind.Core.Entities
{
    public class ClientPreferredTime
    {
        public int ClientId { get; set; }
        public Client Client { get; set; }

        public DaysOfWeek PreferredDay { get; set; }
         public TimeSpan PreferredStartTime { get; set; }
         public TimeSpan PreferredEndTime { get; set; }
    }
}