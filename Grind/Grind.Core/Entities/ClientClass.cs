using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Grind.Core.Entities
{
    public class ClientClass
    {
        public int ClientId { get; set; }
        public Client Client { get; set; }

        public int ClassId { get; set; }
        public Class Class { get; set; }

        public DateTime RegistrationDateTime { get; set; } = DateTime.UtcNow;
    }
}