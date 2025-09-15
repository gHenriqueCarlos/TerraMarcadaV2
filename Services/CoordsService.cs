using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraMarcadaV2.Services
{
    public static class CoordsService
    {
        public static string CoordToDMS(double coord, bool isLatitude)
        {
            int degrees = (int)coord;
            double minutes = (coord - degrees) * 60;
            int intMinutes = (int)minutes;
            double seconds = (minutes - intMinutes) * 60;

            string direction = "";
            if (isLatitude)
            {
                direction = (coord >= 0) ? "N" : "S";
            }
            else
            {
                direction = (coord >= 0) ? "E" : "W";
            }

            return $"{Math.Abs(degrees)}° {Math.Abs(intMinutes)}' {Math.Abs(seconds):F2}\" {direction}";
        }
    }

}
