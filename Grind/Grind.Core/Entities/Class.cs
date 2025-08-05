using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grind.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema; // לשימוש ב-Column

namespace Grind.Core.Entities
{
    public class Class
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentParticipants { get; set; } = 0;
        public DifficultyLevel Difficulty { get; set; }
        public ClassCategory Category { get; set; }
        public bool IsCancelled { get; set; } = false;
        public string? CancellationReason { get; set; }

        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        public ICollection<ClientClass> ClientClasses { get; set; } = new List<ClientClass>();
        public ICollection<WaitingListEntry> WaitingListEntries { get; set; } = new List<WaitingListEntry>();
    }
}