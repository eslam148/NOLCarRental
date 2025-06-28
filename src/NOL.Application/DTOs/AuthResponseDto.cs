namespace NOL.Application.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class AuthRegisterResponseDto
{
    public string Message { get; set; }
}