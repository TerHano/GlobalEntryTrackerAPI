using AutoMapper;
using Business.Dto;
using Business.Dto.Requests;
using Database.Entities;

namespace Business.Mappers;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<UserEntity, UserDto>().ForMember(x => x.Role,
            opt => opt.MapFrom(src => src.UserRole.RoleId));
        CreateMap<CreateUserRequest, UserEntity>();
        CreateMap<UpdateUserRequest, UserEntity>();
    }
}