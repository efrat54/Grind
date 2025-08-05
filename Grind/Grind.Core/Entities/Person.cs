using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Grind.Core.Entities
{
    public abstract class Person
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string HashedPassword { get; set; } // יש לשמור סיסמאות כ-hashed
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int? AddressId { get; set; }
        public Address Address { get; set; }
        public bool IsActive { get; set; } = true;

    }
}