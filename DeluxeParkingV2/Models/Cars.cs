using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeParkingV2.Models
{
    internal class Cars
    {
        public int Id { get; set; }
        public string Plate { get; set; }
        public string Make { get; set; }
        public string Color { get; set; }
        public int? ParkingSlotsId { get; set; }
    }
}
