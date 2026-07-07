using Moq;
using Moveo_backend.Shared.Domain.Repositories;
using Moveo_backend.UserReview.Application.Internal.CommandServices;
using Moveo_backend.UserReview.Domain.Model.Commands;
using Moveo_backend.UserReview.Domain.Repositories;
using Xunit;
using UserReviewEntity = Moveo_backend.UserReview.Domain.Model.Aggregate.UserReview;

namespace Moveo_backend.UnitTests.UserReview;

public class UserReviewCommandServiceTests
{
    private readonly Mock<IUserReviewRepository> _userReviewRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UserReviewCommandService _sut;

    public UserReviewCommandServiceTests()
    {
        _sut = new UserReviewCommandService(_userReviewRepository.Object, _unitOfWork.Object);
    }

    private static UserReviewEntity BuildReview(int id = 1) => new(new CreateUserReviewCommand(
        ReviewerId: 10,
        ReviewedUserId: 20,
        RentalId: 30,
        Rating: 3,
        Comment: "Buen viaje",
        Type: "renter_to_owner"));

    [Fact]
    public async Task Handle_Update_ReturnsNull_WhenReviewDoesNotExist()
    {
        // Arrange
        var command = new UpdateUserReviewCommand(Id: 99, Rating: 5, Comment: "Excelente");
        _userReviewRepository.Setup(r => r.FindByIdAsync(command.Id)).ReturnsAsync((UserReviewEntity?)null);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        Assert.Null(result);
        _userReviewRepository.Verify(r => r.Update(It.IsAny<UserReviewEntity>()), Times.Never);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Update_UpdatesRatingAndComment_WhenReviewExists()
    {
        // Arrange
        var review = BuildReview();
        var command = new UpdateUserReviewCommand(Id: 1, Rating: 5, Comment: "Excelente");
        _userReviewRepository.Setup(r => r.FindByIdAsync(command.Id)).ReturnsAsync(review);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result!.Rating);
        Assert.Equal("Excelente", result.Comment);
        _userReviewRepository.Verify(r => r.Update(review), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Delete_ReturnsFalse_WhenReviewDoesNotExist()
    {
        // Arrange
        var command = new DeleteUserReviewCommand(Id: 99);
        _userReviewRepository.Setup(r => r.FindByIdAsync(command.Id)).ReturnsAsync((UserReviewEntity?)null);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        Assert.False(result);
        _userReviewRepository.Verify(r => r.Remove(It.IsAny<UserReviewEntity>()), Times.Never);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Delete_RemovesReviewAndReturnsTrue_WhenReviewExists()
    {
        // Arrange
        var review = BuildReview();
        var command = new DeleteUserReviewCommand(Id: 1);
        _userReviewRepository.Setup(r => r.FindByIdAsync(command.Id)).ReturnsAsync(review);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        Assert.True(result);
        _userReviewRepository.Verify(r => r.Remove(review), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }
}
