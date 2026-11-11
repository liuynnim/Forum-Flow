using System.Text.Json;
using ForumFlow.Application.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ForumFlow.Api.Controllers;

[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Đăng ký tài khoản mới</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register()
    {
        StreamReader reader = new(Request.Body);
        try
        {
            string body = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                return BadRequest(new { error = "Dữ liệu gửi lên không được để trống!" });
            }

            JsonDocument doc = JsonDocument.Parse(body);

            try
            {
                var root = doc.RootElement;

                if (
                    !root.TryGetProperty("username", out JsonElement usernameProp)
                    || string.IsNullOrWhiteSpace(usernameProp.GetString())
                )
                {
                    return BadRequest(new { error = "Username bắt buộc nhập!" });
                }

                if (
                    !root.TryGetProperty("displayName", out JsonElement displayNameProp)
                    || string.IsNullOrWhiteSpace(displayNameProp.GetString())
                )
                {
                    return BadRequest(new { error = "DisplayName bắt buộc nhập!" });
                }

                if (
                    !root.TryGetProperty("email", out var emailProp)
                    || string.IsNullOrWhiteSpace(emailProp.GetString())
                )
                {
                    return BadRequest(new { error = "Email bắt buộc nhập!" });
                }

                if (
                    !root.TryGetProperty("password", out var passwordProp)
                    || string.IsNullOrWhiteSpace(passwordProp.GetString())
                )
                {
                    return BadRequest(new { error = "Password bắt buộc nhập!" });
                }

                RegisterCommand command = new(
                    usernameProp.GetString()!,
                    displayNameProp.GetString()!,
                    emailProp.GetString()!,
                    passwordProp.GetString()!
                );

                AuthResponseDto result = await _mediator.Send(command);
                return Ok(result);
            }
            finally
            {
                doc.Dispose();
            }
        }
        finally
        {
            reader.Dispose();
        }
    }

    /// <summary>Đăng nhập — trả về JWT Access Token</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        using StreamReader reader = new(Request.Body);
        string body = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
        {
            return BadRequest(new { error = "Dữ liệu gửi lên không được để trống!" });
        }

        using JsonDocument doc = JsonDocument.Parse(body);
        JsonElement root = doc.RootElement;
        if (
            !root.TryGetProperty("email", out var emailProp)
            || string.IsNullOrWhiteSpace(emailProp.GetString())
        )
        {
            return BadRequest(new { error = "Email bắt buộc nhập!" });
        }

        if (
            !root.TryGetProperty("password", out var passwordProp)
            || string.IsNullOrWhiteSpace(passwordProp.GetString())
        )
        {
            return BadRequest(new { error = "Password bắt buộc nhập!" });
        }
        var command = new LoginCommand(emailProp.GetString()!, passwordProp.GetString()!);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
