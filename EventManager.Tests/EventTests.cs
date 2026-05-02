using EventManager.Models.Events;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Tests;

public class EventTests
{
    [Fact]
    [Trait("Category", "Event")]
    public void UnableToCreateEventWithInvalidDates_ThrowsValidationException()
    {
        var eventDto = new EventDto
        {
            Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66af01"),
            Title = "Некорректное событие",
            StartAt = new DateTime(2026, 4, 20, 12, 0, 0),
            EndAt = new DateTime(2026, 4, 20, 11, 0, 0)
        };

        // Act
        var results = ValidateModel(eventDto);
        // Assert
        Assert.Contains(results.SelectMany(r => r.MemberNames), name => name == nameof(EventDto.EndAt));
    }


    private static List<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        return results;
    }
}


