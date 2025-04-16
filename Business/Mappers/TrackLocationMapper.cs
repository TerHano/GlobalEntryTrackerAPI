using AutoMapper;
using Business.Dto.Requests;
using Database.Entities;

namespace Business.Mappers;

public class TrackLocationMapper : Profile
{
    public TrackLocationMapper()
    {
        CreateMap<CreateTrackerForUserRequest, TrackedLocationForUserEntity>();
    }
}