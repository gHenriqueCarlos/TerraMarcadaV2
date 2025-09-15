namespace TerraMarcadaV2.Models;

public class GnssData
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; }
    public double Hdop { get; set; }
    public int SatellitesUsed { get; set; }
    public int SatellitesVisible { get; set; }
    public string FixType { get; set; } = "Sem Fix";
    public DateTime UtcTime { get; set; } = DateTime.UtcNow;

    public List<Satellite> Satellites { get; set; } = new List<Satellite>();
}

public class Satellite
{
    public int SatelliteId { get; set; }
    public double SignalStrength { get; set; }
}