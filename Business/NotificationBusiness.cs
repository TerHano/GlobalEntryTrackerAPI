using AutoMapper;
using Business.Dto;
using Database.Repositories;

namespace Business;

/// <summary>
///     Handles business logic for notification types.
/// </summary>
public class NotificationBusiness(
    NotificationTypeRepository notificationTypeRepository,
    IMapper mapper)
{
    /// <summary>
    ///     Retrieves all notification types.
    /// </summary>
    /// <returns>List of notification type DTOs.</returns>
    public async Task<List<NotificationTypeDto>> GetNotificationAllTypes()
    {
        var notificationTypes = await notificationTypeRepository.GetAllNotificationTypes();
        return mapper.Map<List<NotificationTypeDto>>(notificationTypes);
    }
}