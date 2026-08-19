using UserMicroService.Domain;

namespace UserMicroService.Application;

public static class UserMapper
{
    public static UserInfoDto ToUserInfoDto(User user)
    {
        return new UserInfoDto
        {
            Id = user.Id,
            Login = user.Login,
            Role = user.Role.ToString()
        };
    }
}
