using Grind.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Grind.Core.Entities
{
    public class Trainer : Person
    {
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        public decimal HourlyRate { get; set; }
        public ICollection<ClassCategory> Specializations { get; set; }

        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<TrainerPayment> TrainerPayments { get; set; } = new List<TrainerPayment>();
    }
}