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
        CreateMap<RoleEntity, RoleDto>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(x => x.Code, opt => opt.MapFrom(src => src.Code));
        CreateMap<UserProfileEntity, UserDto>().ForMember(x => x.Role,
            opt => opt.MapFrom(src => src.User.UserRoles.First()));
        CreateMap<UserProfileEntity, UserDtoForAdmin>().ForMember(x => x.Role,
                opt => opt.MapFrom(src => src.User.UserRoles.First()))
            .ForMember(x => x.CustomerId,
                opt => opt.MapFrom(src => src.User.UserCustomer.CustomerId))
            .ForMember(x => x.SubscriptionId,
                opt => opt.MapFrom(src => src.User.UserCustomer.SubscriptionId));
        CreateMap<UserEntity, UserDtoForAdmin>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(x => x.ExternalId, opt => opt.MapFrom(src => src.Id))
            .ForMember(x => x.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(x => x.FirstName, opt => opt.MapFrom(src => src.UserProfile.FirstName))
            .ForMember(x => x.LastName, opt => opt.MapFrom(src => src.UserProfile.LastName))
            .ForMember(x => x.CreatedAt, opt => opt.MapFrom(src => src.UserProfile.CreatedAt))
            .ForMember(x => x.Role, opt => opt.MapFrom(src => src.UserRoles.First()))
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(src => src.UserCustomer.CustomerId))
            .ForMember(x => x.SubscriptionId, opt => opt.MapFrom(src => src.UserCustomer.SubscriptionId))
            .ForMember(x => x.NextNotificationAt, opt => opt.MapFrom(src => src.UserProfile.NextNotificationAt));
        CreateMap<CreateUserRequest, UserProfileEntity>();
        CreateMap<UpdateUserRequest, UserProfileEntity>();
    }
}