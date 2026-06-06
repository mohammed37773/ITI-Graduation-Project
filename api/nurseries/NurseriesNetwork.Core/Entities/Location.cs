using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Entities
{
    public class Location
    {
        public int Id { get; set; }
        public int NurseryId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Navigation Property
        public Nursery Nursery { get; set; } = null!;
    }
}
