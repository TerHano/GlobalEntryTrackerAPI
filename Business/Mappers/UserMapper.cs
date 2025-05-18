using AutoMapper;
using Business.Dto;
using Business.Dto.Requests;
using Database.Entities;

namespace Business.Mappers;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<UserEntity, UserDto>().ForMember(x => x.Roles,
                opt => opt.MapFrom(src => src.UserRoles.Select(r => r.Role.Id).ToList()))
            .ForMember(x => x.NextNotificationAt,
                opt => opt.MapFrom(src => src.NextNotificationAt.ToString("O")));
        CreateMap<CreateUserRequest, UserEntity>();
        CreateMap<UpdateUserRequest, UserEntity>();
    }
}