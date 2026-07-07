using Moq;
using Moveo_backend.Adventure.Application.Internal.CommandServices;
using Moveo_backend.Adventure.Domain.Model;
using Moveo_backend.Adventure.Domain.Model.Commands;
using Moveo_backend.Adventure.Domain.Repositories;
using Moveo_backend.Notification.Domain.Services;
using Moveo_backend.Shared.Domain.Repositories;
using Moveo_backend.UserManagement.Domain.Model.Aggregates;
using Moveo_backend.UserManagement.Domain.Model.Commands;
using Moveo_backend.UserManagement.Domain.Repositories;
using Xunit;
using AdventureRouteEntity = Moveo_backend.Adventure.Domain.Model.Aggregate.AdventureRoute;

namespace Moveo_backend.UnitTests.Adventure;

public class AdventureRouteCommandServiceTests
{
    private readonly Mock<IAdventureRouteRepository> _routeRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRoutePassengerRepository> _passengerRepository = new();
    private readonly Mock<INotificationCommandService> _notificationCommandService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly AdventureRouteCommandService _sut;

    public AdventureRouteCommandServiceTests()
    {
        _sut = new AdventureRouteCommandService(
            _routeRepository.Object,
            _userRepository.Object,
            _passengerRepository.Object,
            _notificationCommandService.Object,
            _unitOfWork.Object);
    }

    private static CreateAdventureRouteCommand BuildClassicRouteCommand(string name = "Ruta al valle") => new(
        OwnerId: 1,
        Name: name,
        Title: "Ruta al valle",
        Description: "Una ruta bonita",
        StartLocation: "Lima",
        EndLocation: "Huaraz",
        Type: "mountain",
        Duration: 8,
        Difficulty: "moderate",
        EstimatedCost: 150m);

    private static CreateAdventureRouteCommand BuildCarpoolRouteCommand(string name = "Carpool a la U") => new(
        OwnerId: 1,
        Name: name,
        Title: "Carpool a la U",
        Description: "Viaje compartido",
        StartLocation: "San Miguel",
        EndLocation: "UPC Monterrico",
        Type: "city",
        Duration: 1,
        Difficulty: "easy",
        EstimatedCost: 0m,
        SeatsTotal: 3);

    private static User BuildUser(string email) => new(new CreateUserCommand(
        FirstName: "Ana",
        LastName: "Perez",
        Email: email,
        Password: "hash",
        Phone: "999999999",
        Dni: "12345678",
        LicenseNumber: "L1",
        Role: "renter"));

    [Fact]
    public async Task Handle_CreatesRoute_WhenNameIsUniqueAndNotCarpool()
    {
        // Arrange
        var command = BuildClassicRouteCommand();
        _routeRepository.Setup(r => r.ExistsByNameAsync(command.Name)).ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Name, result!.Name);
        _routeRepository.Verify(r => r.AddAsync(It.IsAny<AdventureRouteEntity>()), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        _userRepository.Verify(u => u.FindByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ThrowsException_WhenNameAlreadyExists()
    {
        // Arrange
        var command = BuildClassicRouteCommand("Ruta repetida");
        _routeRepository.Setup(r => r.ExistsByNameAsync(command.Name)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.Handle(command));
        _routeRepository.Verify(r => r.AddAsync(It.IsAny<AdventureRouteEntity>()), Times.Never);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ThrowsCarpoolException_WhenOwnerHasNoInstitutionalEmail()
    {
        // Arrange
        var command = BuildCarpoolRouteCommand();
        _routeRepository.Setup(r => r.ExistsByNameAsync(command.Name)).ReturnsAsync(false);
        _userRepository.Setup(u => u.FindByIdAsync(command.OwnerId))
            .ReturnsAsync(BuildUser("ana@gmail.com"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<CarpoolException>(() => _sut.Handle(command));
        Assert.Equal("not_institutional_email", ex.Code);
        _routeRepository.Verify(r => r.AddAsync(It.IsAny<AdventureRouteEntity>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CreatesCarpoolRoute_WhenOwnerHasInstitutionalEmail()
    {
        // Arrange
        var command = BuildCarpoolRouteCommand();
        _routeRepository.Setup(r => r.ExistsByNameAsync(command.Name)).ReturnsAsync(false);
        _userRepository.Setup(u => u.FindByIdAsync(command.OwnerId))
            .ReturnsAsync(BuildUser("ana@upc.edu.pe"));

        // Act
        var result = await _sut.Handle(command);

        // Assert
        Assert.NotNull(result);
        _routeRepository.Verify(r => r.AddAsync(It.IsAny<AdventureRouteEntity>()), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }
}
