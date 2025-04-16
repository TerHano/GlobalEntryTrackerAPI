using AutoMapper;
using Business.Dto;
using Business.Dto.Requests;
using Database.Entities;
using Database.Repositories;
using Service;

namespace Business;

public class UserAppointmentTrackerBusiness(
    TrackedLocationForUserRepository trackedLocationForUserRepository,
    AppointmentLocationRepository appointmentLocationRepository,
    JobService jobService,
    IMapper mapper)
{
    public async Task<TrackedLocationForUserDto> GetTrackedAppointmentLocationById(
        int locationTrackerId, int userId)
    {
        var trackedLocation =
            await trackedLocationForUserRepository.GetTrackerById(locationTrackerId, userId);
        return mapper.Map<TrackedLocationForUserDto>(trackedLocation);
    }

    public async Task<List<AppointmentLocationDto>> GetTrackedAppointmentLocationsForUser(
        int userId)
    {
        var trackedLocations =
            await trackedLocationForUserRepository.GetTrackedLocationsForUser(userId);
        return mapper.Map<List<AppointmentLocationDto>>(trackedLocations);
    }

    public async Task<int> CreateTrackerForUser(CreateTrackerForUserRequest request, int userId)
    {
        var entity = new TrackedLocationForUserEntity
        {
            LocationId = request.LocationId,
            NotificationTypeId = request.NotificationTypeId,
            UserId = userId,
            Enabled = true,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
        // var entity = mapper.Map<TrackedLocationForUserEntity>(request);
        var newTracker = await trackedLocationForUserRepository.CreateTrackerForUser(entity);
        _ = jobService.StartTrackingAppointmentLocation(request.LocationId);
        return newTracker;
    }

    public async Task<int> UpdateTrackerForUser(UpdateTrackerForUserRequest request, int userId)
    {
        var entity = mapper.Map<TrackedLocationForUserEntity>(request);
        entity.UserId = userId;
        return await trackedLocationForUserRepository.UpdateTrackerForUser(entity);
    }

    public async Task<int> DeleteTrackerForUser(int locationTrackerId, int userId)
    {
        return await trackedLocationForUserRepository.DeleteTrackerForUser(locationTrackerId,
            userId);
    }
}