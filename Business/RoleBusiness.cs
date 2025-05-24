using AutoMapper;
using Business.Dto.Admin;
using Database.Repositories;

namespace Business;

public class RoleBusiness(RoleRepository roleRepository, IMapper mapper)
{
    public async Task<List<RoleDto>> GetAllRoles()
    {
        var roles = await roleRepository.GetAllRoles();
        return mapper.Map<List<RoleDto>>(roles);
    }
}