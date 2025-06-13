using AutoMapper;
using Business.Dto;
using Business.Dto.Requests;
using Business.Exceptions;
using Database.Entities;
using Database.Repositories;
using Microsoft.Extensions.Logging;
using Service;

namespace Business;

/// <summary>
///     Handles business logic for tracking user appointment locations.
/// </summary>
public class UserAppointmentTrackerBusiness(
    TrackedLocationForUserRepository trackedLocationForUserRepository,
    UserRepository userRepository,
    JobService jobService,
    IMapper mapper,
    ILogger<UserAppointmentTrackerBusiness> logger)
{
    /// <summary>
    ///     Gets a tracked appointment location by its tracker ID and user ID.
    /// </summary>
    /// <param name="locationTrackerId">Tracker ID.</param>
    /// <param name="userId">User ID.</param>
    /// <returns>Tracked location DTO.</returns>
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

    /// <summary>
    ///     Gets all tracked appointment locations for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>List of tracked location DTOs.</returns>
    public async Task<List<TrackedLocationForUserDto>> GetTrackedAppointmentLocationsForUser(
        int userId)
    {
        var trackedLocations =
            await trackedLocationForUserRepository.GetTrackedLocationsForUser(userId);
        return mapper.Map<List<TrackedLocationForUserDto>>(trackedLocations);
    }

    /// <summary>
    ///     Creates a new tracker for a user.
    /// </summary>
    /// <param name="request">Tracker creation request.</param>
    /// <param name="userId">User ID.</param>
    /// <returns>ID of the new tracker.</returns>
    public async Task<int> CreateTrackerForUser(CreateTrackerForUserRequest request, int userId)
    {
        var user = await userRepository.GetUserById(userId);
        var maxTrackers = user.UserRole.Role.MaxTrackers;
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

    /// <summary>
    ///     Updates an existing tracker for a user.
    /// </summary>
    /// <param name="request">Tracker update request.</param>
    /// <param name="userId">User ID.</param>
    /// <returns>ID of the updated tracker.</returns>
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
        try
        {
            await jobService.StartTrackingAppointmentLocation(request.LocationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            throw new ApplicationException(
                "Error updating tracker, please try again later");
        }

        return await trackedLocationForUserRepository.UpdateTrackerForUser(entity);
    }

    /// <summary>
    ///     Deletes a tracker for a user.
    /// </summary>
    /// <param name="locationTrackerId">Tracker ID.</param>
    /// <param name="userId">User ID.</param>
    /// <returns>ID of the deleted tracker.</returns>
    public async Task<int> DeleteTrackerForUser(int locationTrackerId, int userId)
    {
        return await trackedLocationForUserRepository.DeleteTrackerForUser(locationTrackerId,
            userId);
    }

    /// <summary>
    ///     Deletes all trackers for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    public async Task DeleteAllTrackersForUser(int userId)
    {
        await trackedLocationForUserRepository.DeleteAllTrackersForUser(userId);
    }

    /// <summary>
    ///     Checks if a user already has a tracker for a specific location and notification type.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="locationId">Location ID.</param>
    /// <param name="notificationTypeId">Notification type ID.</param>
    /// <param name="trackerId">Optional tracker ID to exclude from check.</param>
    /// <returns>True if a tracker exists; otherwise, false.</returns>
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