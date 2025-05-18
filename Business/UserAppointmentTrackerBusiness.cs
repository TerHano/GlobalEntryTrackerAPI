using AutoMapper;
using Business.Dto;
using Business.Dto.Requests;
using Database.Entities;
using Database.Repositories;
using GlobalEntryTrackerAPI.Exceptions;
using Microsoft.Extensions.Logging;
using Service;

namespace Business;

public class UserAppointmentTrackerBusiness(
    TrackedLocationForUserRepository trackedLocationForUserRepository,
    UserRepository userRepository,
    JobService jobService,
    IMapper mapper,
    ILogger<UserAppointmentTrackerBusiness> logger)
{
    public async Task<TrackedLocationForUserDto> GetTrackedAppointmentLocationById(
        int locationTrackerId, int userId)
    {
        var trackedLocation =
            await trackedLocationForUserRepository.GetTrackerById(locationTrackerId);
        if (trackedLocation.UserId != userId)
            throw new UnauthorizedAccessException(
                "You are not authorized to access this tracked location.");
        return mapper.Map<TrackedLocationForUserDto>(trackedLocation);
    }

    public async Task<List<TrackedLocationForUserDto>> GetTrackedAppointmentLocationsForUser(
        int userId)
    {
        var trackedLocations =
            await trackedLocationForUserRepository.GetTrackedLocationsForUser(userId);
        return mapper.Map<List<TrackedLocationForUserDto>>(trackedLocations);
    }

    public async Task<int> CreateTrackerForUser(CreateTrackerForUserRequest request, int userId)
    {
        var user = await userRepository.GetUserById(userId);
        var maxTrackers = user.UserRoles.Max(r => r.Role.MaxTrackers);
        var trackedLocationsForUser =
            await trackedLocationForUserRepository.GetTrackedLocationsForUser(userId);
        if (await DoesUserAlreadyHaveTrackerForLocationAndNotificationType(
                userId, request.LocationId, request.NotificationTypeId))
            throw new TrackerForLocationAndTypeExistsException(
                "You already have a tracker for this location and notification type.");
        if (trackedLocationsForUser.Count >= maxTrackers)
            throw new ApplicationException(
                $"You have reached the maximum number of trackers ({maxTrackers}) for your role.");

        var entity = new TrackedLocationForUserEntity
        {
            LocationId = request.LocationId,
            NotificationTypeId = request.NotificationTypeId,
            UserId = userId,
            Enabled = true,
            CutOffDate = request.CutOffDate,
            NextNotificationAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var newTracker = await trackedLocationForUserRepository.CreateTrackerForUser(entity);
        try
        {
            await jobService.StartTrackingAppointmentLocation(request.LocationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            await trackedLocationForUserRepository.DeleteTrackerForUser(newTracker, userId);
            throw new ApplicationException(
                "Error creating tracker, please try again later");
        }

        return newTracker;
    }

    public async Task<int> UpdateTrackerForUser(UpdateTrackerForUserRequest request, int userId)
    {
        var trackedLocation =
            await trackedLocationForUserRepository.GetTrackerById(request.Id);
        if (trackedLocation.UserId != userId)
            throw new UnauthorizedAccessException(
                "You are not authorized to access this tracked location.");
        if (await DoesUserAlreadyHaveTrackerForLocationAndNotificationType(
                userId, request.LocationId, request.NotificationTypeId, request.Id))
            throw new TrackerForLocationAndTypeExistsException(
                "You already have a tracker for this location and notification type.");
        var entity = mapper.Map(request, trackedLocation);
        entity.UpdatedAt = DateTime.UtcNow;
        return await trackedLocationForUserRepository.UpdateTrackerForUser(entity);
    }

    public async Task<int> DeleteTrackerForUser(int locationTrackerId, int userId)
    {
        return await trackedLocationForUserRepository.DeleteTrackerForUser(locationTrackerId,
            userId);
    }

    private async Task<bool> DoesUserAlreadyHaveTrackerForLocationAndNotificationType(
        int userId, int locationId, int notificationTypeId, int trackerId = 0)
    {
        var trackedLocations = await
            trackedLocationForUserRepository.GetTrackedLocationsForUser(userId);
        return trackedLocations.Any(x =>
            x.Id != trackerId &&
            x.LocationId == locationId && x.NotificationTypeId == notificationTypeId);
    }
}