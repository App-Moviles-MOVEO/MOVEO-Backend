using System.Text.Json;
using Moveo_backend.Adventure.Domain.Model.Commands;

namespace Moveo_backend.Adventure.Domain.Model.Aggregate;

/// <summary>
///     Adventure Route Aggregate Root
/// </summary>
public class AdventureRoute
{
    public int Id { get; }
    public int OwnerId { get; private set; }
    public string Name { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string StartLocation { get; private set; }
    public string EndLocation { get; private set; }
    public string Type { get; private set; } // "beach" | "mountain" | "city" | "desert" | "jungle"
    public int Duration { get; private set; } // En horas
    public string Difficulty { get; private set; } // "easy" | "moderate" | "hard"
    public decimal EstimatedCost { get; private set; }
    public string? VehicleName { get; private set; }
    public string? ImageUrl { get; private set; }
    public string TagsJson { get; private set; } = "[]";
    public bool Featured { get; private set; }
    public int? MaxCapacity { get; private set; }
    public double Rating { get; private set; }
    public int ReviewsCount { get; private set; }

    // -------------------- Campos de Carpooling (reuso de esta tabla, opción B) --------------------
    // Esta entidad sirve tanto para "rutas de aventura" como para "viajes compartidos (carpool)".
    // Origin/Destination de carpool ≈ StartLocation/EndLocation. PricePerSeat ≈ por asiento.
    public DateTime? DepartureDate { get; private set; }      // fecha del viaje
    public string? DepartureTime { get; private set; }        // hora "HH:mm"
    public int? SeatsTotal { get; private set; }              // asientos ofrecidos
    public int? SeatsAvailable { get; private set; }          // asientos libres restantes
    public decimal? PricePerSeat { get; private set; }        // precio por asiento
    public bool OnlyWomen { get; private set; }               // viaje solo para mujeres
    public string? Community { get; private set; }            // comunidad/grupo
    public double? Lat { get; private set; }                  // punto de partida (lat)
    public double? Lng { get; private set; }                  // punto de partida (lng)
    public string Status { get; private set; } = "active";    // "active" | "full" | "in_progress" | "cancelled" | "completed"
    public string? RecurrenceGroupId { get; private set; }     // US17 — agrupa las ocurrencias de una serie semanal

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public List<string> Tags
    {
        get => JsonSerializer.Deserialize<List<string>>(TagsJson) ?? new List<string>();
        private set => TagsJson = JsonSerializer.Serialize(value);
    }

    // Constructor vacío para EF Core
    public AdventureRoute()
    {
        Name = string.Empty;
        Title = string.Empty;
        Description = string.Empty;
        StartLocation = string.Empty;
        EndLocation = string.Empty;
        Type = string.Empty;
        Difficulty = string.Empty;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public AdventureRoute(
        int ownerId,
        string name,
        string title,
        string description,
        string startLocation,
        string endLocation,
        string type,
        int duration,
        string difficulty,
        decimal estimatedCost,
        string? vehicleName,
        string? imageUrl,
        List<string>? tags,
        bool featured,
        int? maxCapacity) : this()
    {
        OwnerId = ownerId;
        Name = name;
        Title = title;
        Description = description;
        StartLocation = startLocation;
        EndLocation = endLocation;
        Type = type;
        Duration = duration;
        Difficulty = difficulty;
        EstimatedCost = estimatedCost;
        VehicleName = vehicleName;
        ImageUrl = imageUrl;
        Tags = tags ?? new List<string>();
        Featured = featured;
        MaxCapacity = maxCapacity;
        Rating = 0;
        ReviewsCount = 0;
    }

    public AdventureRoute(CreateAdventureRouteCommand command) : this()
    {
        OwnerId = command.OwnerId;
        Name = command.Name;
        Title = command.Title;
        Description = command.Description;
        StartLocation = command.StartLocation;
        EndLocation = command.EndLocation;
        Type = command.Type;
        Duration = command.Duration;
        Difficulty = command.Difficulty;
        EstimatedCost = command.EstimatedCost;
        VehicleName = command.VehicleName;
        ImageUrl = command.ImageUrl;
        Tags = command.Tags ?? new List<string>();
        Featured = command.Featured;
        MaxCapacity = command.MaxCapacity;
        Rating = 0;
        ReviewsCount = 0;
        // Carpool
        DepartureDate = command.DepartureDate;
        DepartureTime = command.DepartureTime;
        SeatsTotal = command.SeatsTotal;
        SeatsAvailable = command.SeatsAvailable ?? command.SeatsTotal;
        PricePerSeat = command.PricePerSeat;
        OnlyWomen = command.OnlyWomen;
        Community = command.Community;
        Lat = command.Lat;
        Lng = command.Lng;
        Status = "active";
        RecurrenceGroupId = command.RecurrenceGroupId;
    }

    public void Update(UpdateAdventureRouteCommand command)
    {
        Name = command.Name;
        Title = command.Title;
        Description = command.Description;
        StartLocation = command.StartLocation;
        EndLocation = command.EndLocation;
        Type = command.Type;
        Duration = command.Duration;
        Difficulty = command.Difficulty;
        EstimatedCost = command.EstimatedCost;
        VehicleName = command.VehicleName;
        ImageUrl = command.ImageUrl;
        Tags = command.Tags ?? new List<string>();
        Featured = command.Featured;
        MaxCapacity = command.MaxCapacity;
        // Carpool
        DepartureDate = command.DepartureDate;
        DepartureTime = command.DepartureTime;
        SeatsTotal = command.SeatsTotal;
        SeatsAvailable = command.SeatsAvailable ?? command.SeatsTotal;
        PricePerSeat = command.PricePerSeat;
        OnlyWomen = command.OnlyWomen;
        Community = command.Community;
        Lat = command.Lat;
        Lng = command.Lng;
        if (!string.IsNullOrEmpty(command.Status)) Status = command.Status!;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRating(double newRating, int newReviewsCount)
    {
        Rating = newRating;
        ReviewsCount = newReviewsCount;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reserva una cantidad de asientos de carpool (descuenta de SeatsAvailable).
    /// Lanza si no hay asientos suficientes.
    /// </summary>
    public void BookSeats(int seats)
    {
        if (seats < 1) throw new ArgumentException("Debe reservar al menos 1 asiento");
        if (SeatsAvailable is null)
            throw new InvalidOperationException("Esta ruta no maneja asientos de carpool");
        if (SeatsAvailable < seats)
            throw new InvalidOperationException("No hay asientos disponibles suficientes");

        SeatsAvailable -= seats;
        if (SeatsAvailable <= 0) Status = "full";
        UpdatedAt = DateTime.UtcNow;
    }

    // -------------------- Transiciones de estado de la ruta (US-carpool) --------------------
    private static readonly HashSet<string> StartableStatuses = new() { "active", "full" };

    /// <summary>Inicia la ruta: solo desde active/full. Marca in_progress.</summary>
    public void Start()
    {
        if (!StartableStatuses.Contains(Status))
            throw new InvalidOperationException($"No se puede iniciar una ruta en estado '{Status}'");
        Status = "in_progress";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Completa la ruta: solo desde in_progress.</summary>
    public void CompleteRoute()
    {
        if (Status != "in_progress")
            throw new InvalidOperationException($"Solo se puede completar una ruta en curso (estado actual: '{Status}')");
        Status = "completed";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Cancela la ruta: desde cualquier estado no terminal.</summary>
    public void CancelRoute()
    {
        if (Status is "completed" or "cancelled")
            throw new InvalidOperationException($"No se puede cancelar una ruta en estado '{Status}'");
        Status = "cancelled";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Libera asientos previamente confirmados (al rechazar o quitar un pasajero confirmado).
    /// Nunca excede SeatsTotal y reactiva la ruta si estaba "full".
    /// </summary>
    public void ReleaseSeats(int seats)
    {
        if (seats < 1) return;
        if (SeatsAvailable is null) return;

        SeatsAvailable += seats;
        if (SeatsTotal.HasValue && SeatsAvailable > SeatsTotal.Value)
            SeatsAvailable = SeatsTotal.Value;
        if (Status == "full" && SeatsAvailable > 0)
            Status = "active";
        UpdatedAt = DateTime.UtcNow;
    }
}
