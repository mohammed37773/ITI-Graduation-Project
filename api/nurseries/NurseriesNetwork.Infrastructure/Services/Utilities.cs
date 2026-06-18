using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Infrastructure.Services
{
    public static class Utilities
    {
        public static double ToRad(double degrees) => degrees * Math.PI / 180;

        public static double GetDistanceKm(double lat1, double lng1, double lat2, double lng2)
        {
            const double EarthRadiusKm = 6371;
            double dLat = ToRad(lat2 - lat1);
            double dLon = ToRad(lng2 - lng1);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) *
                Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }
    }
}
