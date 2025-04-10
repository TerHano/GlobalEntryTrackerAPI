using AutoMapper;
using Business.Dto;
using Business.Dto.Requests;
using Database.Entities;

namespace Business.Mappers;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<UserEntity, UserDto>().ReverseMap();
        CreateMap<CreateUserRequest, UserEntity>();
    }
}