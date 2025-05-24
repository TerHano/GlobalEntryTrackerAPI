using AutoMapper;
using Business.Dto.Admin;
using Database.Entities;

namespace Business.Mappers;

public class RoleMapper : Profile
{
    public RoleMapper()
    {
        CreateMap<RoleEntity, RoleDto>();
    }
}