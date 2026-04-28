using EventManager.Interfaces;
using EventManager.Models.Event;
using EventManager.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace EventManager.Tests;

public class EventTests
{
    [Fact]
    [Trait("Category", "Event")]
    [Trait("Subcategory", "EventDto")]
    public void UnableToCreateEventWithInvalidDates_ThrowsValidationException()
    {
        var eventDto = new EventDto
        {
            Id = 1,
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


