namespace Moveo_backend.Rental.Domain.Model.ValueObjects;

public record Location
{
    public string District { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public double Lat { get; init; }
    public double Lng { get; init; }
    
    protected Location() { }
    
    public Location(string district, string address, double lat, double lng)
    {
        District = district ?? string.Empty;
        Address = address ?? string.Empty;
        Lat = lat;
        Lng = lng;
    }

    public override string ToString() => $"{Address}, {District}";
}