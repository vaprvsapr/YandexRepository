using EventManager.Domain.Models;
using EventManager.Application.Dto;

namespace EventManager.Application.Mappers;

public static class UserMapper
{
    public static UserInfoDto ToUserInfoDto(User user)
    {
        return new UserInfoDto
        {
            Id = user.Id,
            Login = user.Login
        };
    }
}
