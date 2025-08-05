using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grind.Core.DTOs;
using Grind.Core.Enums;

namespace Grind.Core.Entities
{
    public class ClientPreferredClass
    {
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public ClassCategory ClassCategory { get; set; }
    }
}