namespace UserMicroService.Application;

public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Login { get; set; } = null!;
    public string Role { get; set; } = null!;
}
