using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Dto;

public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Login { get; set; } = null!;

}
