using TerraMarcadaV2.Models;

namespace TerraMarcadaV2.Services;

public class NmeaParser
{
    public GnssData Current { get; private set; } = new();

    public void Parse(string line)
    {
        var parts = line.Split(',');
        if (parts.Length < 6) return;

        if (line.StartsWith("$GPGGA"))
            ParseGGA(parts);
        else if (line.StartsWith("$GPRMC"))
            ParseRMC(parts);
        else if (line.StartsWith("$GPGSV"))
            ParseGSV(parts);
    }

    private void ParseGGA(string[] p)
    {
        Current.Latitude = ConvertToDecimal(p[2], p[3]);
        Current.Longitude = ConvertToDecimal(p[4], p[5]);
        Current.FixType = FixDescription(p[6]);
        Current.SatellitesUsed = int.TryParse(p[7], out var s) ? s : 0;
        Current.Hdop = double.TryParse(p[8], out var h) ? h : 0;
        Current.Altitude = double.TryParse(p[9], out var a) ? a : 0;
    }

    private void ParseRMC(string[] p)
    {
        if (DateTime.TryParseExact(p[1], "HHmmss", null,
            System.Globalization.DateTimeStyles.None, out var t))
        {
            Current.UtcTime = DateTime.UtcNow.Date + t.TimeOfDay;
        }
    }

    //private void ParseGSV(string[] p)
    //{
    //    if (int.TryParse(p[3], out var vis))
    //        Current.SatellitesVisible = vis;
    //}

    private void ParseGSV(string[] p)
    {
        if (int.TryParse(p[3], out var vis))
            Current.SatellitesVisible = vis;

        // Adiciona os satélites à lista
        for (int i = 4; i < p.Length; i += 4)
        {
            if (int.TryParse(p[i], out var satelliteId))
            {
                var signalStrength = p[i + 3]; // A força do sinal está no índice 7
                Current.Satellites.Add(new Satellite
                {
                    SatelliteId = satelliteId,
                    SignalStrength = double.TryParse(signalStrength, out var signal) ? signal : 0
                });
            }
        }
    }


    private static double ConvertToDecimal(string value, string hemi)
    {
        if (string.IsNullOrEmpty(value)) return 0;
        double d = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        double deg = Math.Floor(d / 100);
        double min = d - (deg * 100);
        double result = deg + (min / 60);
        if (hemi == "S" || hemi == "W") result *= -1;
        return result;
    }

    private static string FixDescription(string fix)
    {
        return fix switch
        {
            "0" => "Sem fix",
            "1" => "GPS fix",
            "2" => "DGPS fix",
            "4" => "RTK Fix",
            "5" => "RTK Float",
            _ => "Desconhecido"
        };
    }
}