using Microsoft.AspNetCore.Mvc;
using Moq;
using NOL.API.Controllers;
using NOL.Application.Common.Interfaces;
using NOL.Application.DTOs;
using NOL.Domain.Entities;
using Xunit;

namespace NOL.Application.Tests.Features.BookingCost;

public class BookingCostCalculationTests
{
    private readonly Mock<ICarRepository> _mockCarRepository;
    private readonly Mock<IBookingRepository> _mockBookingRepository;
    private readonly Mock<ILoyaltyPointService> _mockLoyaltyPointService;
    private readonly Mock<IBranchRepository> _mockBranchRepository;
    private readonly BookingController _controller;

    public BookingCostCalculationTests()
    {
        _mockCarRepository = new Mock<ICarRepository>();
        _mockBookingRepository = new Mock<IBookingRepository>();
        _mockLoyaltyPointService = new Mock<ILoyaltyPointService>();
        _mockBranchRepository = new Mock<IBranchRepository>();

        _controller = new BookingController(
            _mockCarRepository.Object,
            _mockBookingRepository.Object,
            _mockLoyaltyPointService.Object,
            _mockBranchRepository.Object);
    }

    [Fact]
    public async Task CalculateBookingCost_ValidRequest_ReturnsCorrectCost()
    {
        // Arrange
        var request = new BookingCostCalculationRequestDto
        {
            CarId = 1,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(4), // 3 days
            PickupBranchId = 1,
            ReturnBranchId = 1,
            ExtraIds = new List<int> { 1 },
            LoyaltyPointsToRedeem = null
        };

        var car = new Car
        {
            Id = 1,
            BrandEn = "Toyota",
            ModelEn = "Camry",
            DailyRate = 100m
        };

        var extra = new ExtraTypePrice
        {
            Id = 1,
            NameEn = "GPS",
            NameAr = "نظام تحديد المواقع",
            DailyPrice = 20m
        };

        _mockCarRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(car);

        _mockBookingRepository.Setup(x => x.IsCarAvailableAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        _mockBookingRepository.Setup(x => x.GetExtraTypePricesByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<ExtraTypePrice> { extra });

        // Act
        var result = await _controller.CalculateBookingCost(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<BookingCostCalculationResponseDto>(okResult.Value);

        Assert.True(response.IsAvailable);
        Assert.Equal(1, response.CarId);
        Assert.Equal("Toyota Camry", response.CarName);
        Assert.Equal(3, response.TotalDays);
        Assert.Equal(300m, response.BaseCost); // 100 * 3 days
        Assert.Equal(60m, response.TotalExtrasCost); // 20 * 3 days
        Assert.Equal(15m, response.InsuranceFee); // 5% of 300
        Assert.Equal(0m, response.DeliveryFee); // Same branch
        Assert.Equal("SAR", response.Currency);
    }

    [Fact]
    public async Task CalculateBookingCost_CarNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new BookingCostCalculationRequestDto
        {
            CarId = 999,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PickupBranchId = 1,
            ReturnBranchId = 1
        };

        _mockCarRepository.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Car)null);

        // Act
        var result = await _controller.CalculateBookingCost(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = notFoundResult.Value;
        Assert.NotNull(response);
    }

    [Fact]
    public async Task CalculateBookingCost_CarUnavailable_ReturnsUnavailableResponse()
    {
        // Arrange
        var request = new BookingCostCalculationRequestDto
        {
            CarId = 1,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PickupBranchId = 1,
            ReturnBranchId = 1
        };

        var car = new Car
        {
            Id = 1,
            BrandEn = "Toyota",
            ModelEn = "Camry",
            DailyRate = 100m
        };

        _mockCarRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(car);

        _mockBookingRepository.Setup(x => x.IsCarAvailableAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CalculateBookingCost(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<BookingCostCalculationResponseDto>(okResult.Value);

        Assert.False(response.IsAvailable);
        Assert.Equal("Car is not available for the selected dates", response.UnavailabilityReason);
        Assert.Equal(0m, response.FinalAmount);
    }

    [Fact]
    public async Task CalculateBookingCost_InvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        var request = new BookingCostCalculationRequestDto
        {
            CarId = 1,
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(1), // End before start
            PickupBranchId = 1,
            ReturnBranchId = 1
        };

        // Act
        var result = await _controller.CalculateBookingCost(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task CalculateBookingCost_LongTermRental_AppliesDiscount()
    {
        // Arrange
        var request = new BookingCostCalculationRequestDto
        {
            CarId = 1,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(8), // 7 days - qualifies for discount
            PickupBranchId = 1,
            ReturnBranchId = 1
        };

        var car = new Car
        {
            Id = 1,
            BrandEn = "Toyota",
            ModelEn = "Camry",
            DailyRate = 100m
        };

        _mockCarRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(car);

        _mockBookingRepository.Setup(x => x.IsCarAvailableAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        _mockBookingRepository.Setup(x => x.GetExtraTypePricesByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<ExtraTypePrice>());

        // Act
        var result = await _controller.CalculateBookingCost(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<BookingCostCalculationResponseDto>(okResult.Value);

        Assert.True(response.IsAvailable);
        Assert.True(response.Discounts.Any());
        Assert.Contains(response.Discounts, d => d.DiscountType == "LongTermDiscount");
        Assert.True(response.TotalDiscountAmount > 0);
    }

    [Fact]
    public async Task CalculateBookingCost_DifferentBranches_AppliesDeliveryFee()
    {
        // Arrange
        var request = new BookingCostCalculationRequestDto
        {
            CarId = 1,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PickupBranchId = 1,
            ReturnBranchId = 2 // Different branch
        };

        var car = new Car
        {
            Id = 1,
            BrandEn = "Toyota",
            ModelEn = "Camry",
            DailyRate = 100m
        };

        _mockCarRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(car);

        _mockBookingRepository.Setup(x => x.IsCarAvailableAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        _mockBookingRepository.Setup(x => x.GetExtraTypePricesByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<ExtraTypePrice>());

        // Act
        var result = await _controller.CalculateBookingCost(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<BookingCostCalculationResponseDto>(okResult.Value);

        Assert.True(response.IsAvailable);
        Assert.Equal(50m, response.DeliveryFee); // Fixed delivery fee
    }

    [Theory]
    [InlineData(1, 100)] // 1 day
    [InlineData(3, 300)] // 3 days
    [InlineData(7, 700)] // 7 days
    public async Task CalculateBookingCost_DifferentDurations_CalculatesCorrectBaseCost(int days, decimal expectedBaseCost)
    {
        // Arrange
        var request = new BookingCostCalculationRequestDto
        {
            CarId = 1,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(1 + days),
            PickupBranchId = 1,
            ReturnBranchId = 1
        };

        var car = new Car
        {
            Id = 1,
            BrandEn = "Toyota",
            ModelEn = "Camry",
            DailyRate = 100m
        };

        _mockCarRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(car);

        _mockBookingRepository.Setup(x => x.IsCarAvailableAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        _mockBookingRepository.Setup(x => x.GetExtraTypePricesByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<ExtraTypePrice>());

        // Act
        var result = await _controller.CalculateBookingCost(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<BookingCostCalculationResponseDto>(okResult.Value);

        Assert.True(response.IsAvailable);
        Assert.Equal(expectedBaseCost, response.BaseCost);
        Assert.Equal(days, response.TotalDays);
    }
}
