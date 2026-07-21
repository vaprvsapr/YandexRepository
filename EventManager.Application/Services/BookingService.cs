using EventManager.Domain.Models;
using EventManager.Domain.Exceptions;
using EventManager.Application.Dto;
using EventManager.Application.Repositories;
using EventManager.Application.Mappers;
using EventManager.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Security.Authentication;

namespace EventManager.Application.Services;

/// <summary>
/// Сервис для управления бронированиями событий, реализующий бизнес-логику создания, получения и поиска бронирований.
/// </summary>
/// <param name="bookingRepository">Репозиторий бронирований.</param>
/// <param name="eventRepository">Репозиторий событий.</param>
/// <param name="userRepository">Репозиторий пользователей.</param>
/// <param name="logger">Логгер для записи информации о процессе управления бронированиями.</param>
public class BookingService(
    IBookingRepository bookingRepository,
    IEventRepository eventRepository,
    IUserRepository userRepository,
    ILogger<BookingService> logger,
    IConfiguration configuration) : IBookingService
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ILogger<BookingService> _logger = logger;
    private readonly int _maxActiveBookingsPerUser = int.Parse(configuration["User:MaxActiveBookings"] ?? "1"); // Максимальное количество активных бронирований на пользователя
    private readonly SemaphoreSlim _bookingSemaphore = new(1, 1); // Семафор для синхронизации доступа к бронированию мест

    /// <inheritdoc/>
    public async Task<BookingDto> CreateAsync(Guid eventId, Guid userId)
    {
        Event existingEvent = await GetEventByIdAsync(eventId);
        User existingUser = await GetUserByIdAsync(userId);

        Booking newBooking;

        await _bookingSemaphore.WaitAsync();
        try
        {
            if (existingEvent.StartAt <= DateTime.UtcNow)
                throw new PastEventBookingException($"Невозможно создать бронирование для события с id: {eventId}, так как оно уже началось или завершилось.");

            var activeBookingsCount = _bookingRepository.GetAll()
                .Where(b => b.UserId == userId && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending))
                .Count(b => b.Event.StartAt > DateTime.UtcNow);
            if (activeBookingsCount >= _maxActiveBookingsPerUser)
                throw new ExceedingActiveBookingLimitException($"Пользователь с id: {userId} превысил лимит активных бронирований. Лимит: {_maxActiveBookingsPerUser}");

            if (!existingEvent.TryReserveSeats())
                throw new NoAvailableSeatsException($"Нет достаточного количества свободных мест на событие с id: {eventId}.");
            await _eventRepository.UpdateAsync(existingEvent);

            newBooking = new()
            { 
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await _bookingRepository.CreateAsync(newBooking);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Created new booking with Id:{id} for EventId:{eventId}.",
                    newBooking.Id, newBooking.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании бронирования для EventId:{eventId} и UserId:{userId}.", eventId, userId);
            throw;
        }
        finally
        {
            _bookingSemaphore.Release();
        }

        return BookingMapper.ToBookingDto(newBooking);
    }

    /// <inheritdoc/>   
    public async Task<BookingDto?> GetByIdAsync(Guid id)
    {
        return BookingMapper.ToBookingDto(await GetBookingByIdAsync(id));
    }

    /// <inheritdoc/>
    public async Task<List<BookingDto>> GetAllBookingsAsync()
    {
        return [.. _bookingRepository
            .GetAll()
            .Select(BookingMapper.ToBookingDto)
            ];
    }

    public async Task<BookingDto> CancelByIdAsync(Guid bookingId, UserInfoDto userInfoDto)
    {
        await ValidateUserCredentials(userInfoDto);
        if (userInfoDto.Role != UserRole.Admin.ToString() && 
            userInfoDto.Role != UserRole.User.ToString())
            throw new UnauthorizedAccessException(
                $"Пользователь с ролью {userInfoDto.Role} не имеет прав на отмену бронирования.");

        var existingBooking = await GetBookingByIdAsync(bookingId);
        if (existingBooking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException($"Событие с Id:{bookingId} уже отменено.");
        if (existingBooking.Status == BookingStatus.Rejected)
            throw new InvalidOperationException($"Событие с Id:{bookingId} отклонено.");
        await _bookingRepository.CancelAsync(existingBooking);

        var existingEvent = await GetEventByIdAsync(existingBooking.EventId);
        existingEvent.ReleaseSeats();
        await _eventRepository.UpdateAsync(existingEvent);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Cancelled booking with Id:{id}.", bookingId);

        return BookingMapper.ToBookingDto(existingBooking);
    }

    private async Task<Booking> GetBookingByIdAsync(Guid id)
    {
        var existingBooking = await _bookingRepository.GetByIdAsync(id) ??
            throw new KeyNotFoundException($"Бронирование с Id:{id} не найдено.");
        return existingBooking;
    }

    private async Task<Event> GetEventByIdAsync(Guid id)
    {
        var existingEvent = await _eventRepository.GetByIdAsync(id) ??
            throw new KeyNotFoundException($"Событие с Id:{id} не найдено.");
        return existingEvent;
    }

    private async Task<User> GetUserByIdAsync(Guid id)
    {
        var existingUser = await _userRepository.GetByIdAsync(id) ??
            throw new KeyNotFoundException($"Пользователь с Id:{id} не найден.");
        return existingUser;
    }

    /// ??? Вопрос по хэшу пароля в токене. Если токен украли и пользователь изменил пароль, то нужно заблокировать старый токен.
    private async Task ValidateUserCredentials(UserInfoDto userInfoDto)
    {
        var existingUser = await GetUserByIdAsync(userInfoDto.Id);
        if (existingUser.Role != Enum.Parse<UserRole>(userInfoDto.Role) || 
            existingUser.Login != userInfoDto.Login)
            throw new InvalidCredentialException("Неверные учетные данные пользователя.");
    }
}
