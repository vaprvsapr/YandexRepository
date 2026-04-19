using EventManager.Interfaces;
using EventManager.Models;
using EventManager.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventManager.Tests;

public class EventTests
{
    [Fact]
    [Trait("Category", "Event")]
    [Trait("Subcategory", "EventPutDto")]
    public void UnableToCreateEventWithInvalidDates_ThrowsValidationException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ValidationException>(CreateInvalidEventDto);
    }

    public static EventDto CreateInvalidEventDto()
    {
        return new EventDto
        {
            Id = 0,
            Title = "Invalid Event",
            StartAt = new DateTime(1),
            EndAt = new DateTime(0)
        };
    }
}
