﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeParkingV2.Models
{
    internal class ParkedCars
    {
        public int Id { get; set; }
        public string Plate { get; set; }
        public string Color { get; set; }
        public string HouseName { get; set; }
        public string ParkingSlot { get; set; }
        public int ElectricOutlet { get; set; }
    }
}
