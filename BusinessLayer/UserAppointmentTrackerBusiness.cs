using AutoMapper;
using BusinessLayer.Dto.Requests;
using Database.Entities;
using Database.Repositories;

namespace BusinessLayer;

public class UserAppointmentTrackerBusiness(TrackedLocationForUserRepository trackedLocationForUserRespository, IMapper mapper)
{
    public void CreateTrackerForUser(CreateTrackerForUserRequest request, int userId)
    {
        var entity = mapper.Map<TrackedLocationForUserEntity>(request);
        entity.UserId = userId;
        trackedLocationForUserRespository.CreateTracker(entity);
    }
}