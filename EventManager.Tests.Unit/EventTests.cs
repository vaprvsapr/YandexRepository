using EventManager.Application.Dto;
using EventManager.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Tests.Unit;

public class EventTests
{
    [Fact]
    [Trait("Category", "Event")]
    public void UnableToCreateEventWithInvalidDates_ThrowsValidationException()
    {
        var eventDto = new EventCreateDto
        {
            Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af01"),
            Title = "Некорректное событие",
            StartAt = new DateTime(2026, 4, 20, 12, 0, 0),
            EndAt = new DateTime(2026, 4, 20, 11, 0, 0),
            TotalSeats = 10
        };

        // Act
        var results = ValidateModel(eventDto);
        // Assert
        Assert.Contains(results.SelectMany(r => r.MemberNames), name => name == nameof(EventCreateDto.EndAt));
    }


    private static List<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        return results;
    }

    [Fact]
    [Trait("Category", "Event")]
    public void UnableToCreateEventWithNegativeSeats_ThrowsValidationException()
    {
        var eventDto = new EventCreateDto
        {
            Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af01"),
            Title = "Некорректное событие",
            StartAt = new DateTime(2026, 4, 20, 12, 0, 0),
            EndAt = new DateTime(2026, 4, 20, 13, 0, 0),
            TotalSeats = -5
        };
        // Act
        var results = ValidateModel(eventDto);
        // Assert
        Assert.Contains(results.SelectMany(r => r.MemberNames), name => name == nameof(EventCreateDto.TotalSeats));
    }

    [Fact]
    [Trait("Category", "Event")]
    public void ReleaseSeats_UnableToReleaseMoreSeatsThanBooked_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af01"),
            Title = "Тестовое событие",
            Description = "Описание тестового события",
            StartAt = new DateTime(2026, 4, 20, 12, 0, 0),
            EndAt = new DateTime(2026, 4, 20, 13, 0, 0),
            TotalSeats = 10,
            AvailableSeats = 9
        };
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => eventEntity.ReleaseSeats(2));
    }
}


