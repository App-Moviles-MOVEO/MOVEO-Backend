namespace Moveo_backend.Rental.Domain.Model.Aggregates;

/// <summary>
/// Última posición conocida del viaje de un alquiler (US06/US07). Se mantiene un registro por
/// rental (se sobrescribe con cada ping) para tracking en tiempo real desde la app.
/// </summary>
public class TripLocation
{
    public int Id { get; private set; }
    public int RentalId { get; private set; }
    public double Lat { get; private set; }
    public double Lng { get; private set; }
    public double? Heading { get; private set; }   // rumbo en grados (0-360)
    public double? Speed { get; private set; }      // velocidad en km/h
    public double? Progress { get; private set; }   // avance 0..1 hacia el destino
    public string? EtaText { get; private set; }     // ETA legible ("12 min")
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    protected TripLocation() { }

    public TripLocation(int rentalId, double lat, double lng)
    {
        RentalId = rentalId;
        Update(lat, lng, null, null, null, null);
    }

    public void Update(double lat, double lng, double? heading, double? speed, double? progress, string? etaText)
    {
        Lat = lat;
        Lng = lng;
        Heading = heading;
        Speed = speed;
        Progress = progress;
        EtaText = etaText;
        UpdatedAt = DateTime.UtcNow;
    }
}
