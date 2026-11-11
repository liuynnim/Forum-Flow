using ForumFlow.Application.Common.Interfaces;
using ForumFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ForumFlow.Application.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(IApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(
        LoginCommand request,
        CancellationToken cancellationToken
    )
    {
        User user =
            await _context.Users.FirstOrDefaultAsync(
                u => u.Email == request.Email && u.DeletedAt == null,
                cancellationToken
            ) ?? throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        if (user.IsBanned)
            throw new UnauthorizedAccessException("Tài khoản của bạn đã bị khóa.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        return new AuthResponseDto(
            user.Id,
            user.Username,
            user.DisplayName,
            user.Email,
            user.Role.ToString(),
            accessToken
        );
    }
}
