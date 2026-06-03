namespace Moveo_backend.Rental.Interfaces.REST.Resources;

public record UpdateReviewResource(
    int? Rating,
    string? Comment);
