using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeParkingV2.Models
{
    internal class ParkingSlots
    {
        public int Id { get; set; }
        public int SlotNumber { get; set; }
        public int ParkingHouseId { get; set; }
    }
}
