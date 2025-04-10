using AutoMapper;
using Business.Dto;
using Database.Repositories;

namespace Business;

public class NotificationBusiness(
    NotificationTypeRepository notificationTypeRepository,
    IMapper mapper)
{
    public async Task<List<NotificationTypeDto>> GetNotificationAllTypes()
    {
        var notificationTypes = await notificationTypeRepository.GetAllNotificationTypes();
        return mapper.Map<List<NotificationTypeDto>>(notificationTypes);
    }
}