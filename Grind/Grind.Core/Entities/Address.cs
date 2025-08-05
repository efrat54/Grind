using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Grind.Core.Entities
{
    public class Address
    {
        public int Id { get; set; }
        public string City { get; set; }
        public string Street { get; set; }

        public string ApartmentNumber { get; set; }
        public Address() { }
        public Address(string city, string street, string apartmentNumber)
        {
            this.City = city;
            this.Street = street;
            this.ApartmentNumber = apartmentNumber;
        }
        public override string ToString()
        {
            return $"{Street}, {ApartmentNumber}, {City}";
        }
        // קשרים:
        // public int? PersonId { get; set; } // אם הכתובת קשורה לאדם ספציפי
        // public Person Person { get; set; }
    }
}