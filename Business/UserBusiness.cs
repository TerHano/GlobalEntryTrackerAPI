using AutoMapper;
using Business.Dto.Requests;
using Database.Entities;
using Database.Repositories;

namespace Business;

public class UserBusiness(UserRepository userRepository, IMapper mapper)
{
    public async Task CreateUser(CreateUserRequest request)
    {
        var newUser = mapper.Map<UserEntity>(request);
        await userRepository.CreateUser(newUser);
    }
}