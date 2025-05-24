using AutoMapper;
using Business.Dto;
using Business.Dto.Admin;
using Business.Dto.Requests;
using Database.Entities;

namespace Business.Mappers;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<UserEntity, UserDto>().ForMember(x => x.Role,
            opt => opt.MapFrom(src => src.UserRole.Role));
        CreateMap<UserEntity, UserDtoForAdmin>().ForMember(x => x.Role,
                opt => opt.MapFrom(src => src.UserRole.Role))
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(src => src.UserCustomer.CustomerId))
            .ForMember(x => x.SubscriptionId,
                opt => opt.MapFrom(src => src.UserCustomer.SubscriptionId));
        CreateMap<CreateUserRequest, UserEntity>();
        CreateMap<UpdateUserRequest, UserEntity>();
    }
}