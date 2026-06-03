namespace Moveo_backend.Rental.Domain.Model.Commands;

public record UpdateReviewCommand(
    int Id,
    int? Rating,
    string? Comment);
