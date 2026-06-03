namespace Moveo_backend.Rental.Interfaces.REST.Resources;

/// <summary>
/// Cuerpo para pagar una reserva en un solo paso (POST /rentals/{id}/pay).
/// Si no se envía amount, se usa el TotalPrice de la reserva.
/// paymentMethod por defecto "yape".
/// </summary>
public record PayRentalResource(
    string? PaymentMethod = "yape",
    decimal? Amount = null,
    string? Currency = "PEN",
    string? Type = "rental_payment",
    string? TransactionId = null,
    string? Description = null
);
