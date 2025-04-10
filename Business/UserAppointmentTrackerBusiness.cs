using AutoMapper;
using Business.Dto;
using Business.Dto.Requests;
using Database.Entities;
using Database.Repositories;

namespace Business;

public class UserAppointmentTrackerBusiness(
    TrackedLocationForUserRepository trackedLocationForUserRepository,
    IMapper mapper)
{
    public async Task<List<AppointmentLocationDto>> GetTrackedAppointmentLocationsForUser(
        int userId)
    {
        var trackedLocations =
            await trackedLocationForUserRepository.GetTrackedLocationsForUser(userId);
        return mapper.Map<List<AppointmentLocationDto>>(trackedLocations);
    }

    public async Task<int> CreateTrackerForUser(CreateTrackerForUserRequest request, int userId)
    {
        var entity = mapper.Map<TrackedLocationForUserEntity>(request);
        entity.UserId = userId;
        return await trackedLocationForUserRepository.CreateTracker(entity);
    }

    public async Task<int> UpdateTrackerForUser(UpdateTrackerForUserRequest request, int userId)
    {
        var entity = mapper.Map<TrackedLocationForUserEntity>(request);
        entity.UserId = userId;
        return await trackedLocationForUserRepository.UpdateTracker(entity);
    }

    public async Task<int> DeleteTrackerForUser(int locationTrackerId, int userId)
    {
        return await trackedLocationForUserRepository.DeleteTracker(locationTrackerId, userId);
    }
}