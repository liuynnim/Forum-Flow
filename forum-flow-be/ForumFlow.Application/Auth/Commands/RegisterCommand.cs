using ForumFlow.Application.Common.Interfaces;
using ForumFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ForumFlow.Application.Auth.Commands;

public record RegisterCommand(string Username, string DisplayName, string Email, string Password)
    : IRequest<AuthResponseDto>;

public record AuthResponseDto(
    Guid UserId,
    string Username,
    string DisplayName,
    string Email,
    string Role,
    string AccessToken
);

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(IApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken
    )
    {
        if (
            await _context.Users.AnyAsync(
                u => u.Email == request.Email || u.Username == request.Username,
                cancellationToken
            )
        )
        {
            throw new InvalidOperationException("Username hoặc Email đã tồn tại.");
        }

        User user = new User
        {
            Username = request.Username,
            DisplayName = request.DisplayName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        string accessToken = _jwtTokenService.GenerateAccessToken(user);

        AuthResponseDto dto = new AuthResponseDto(
            user.Id,
            user.Username,
            user.DisplayName,
            user.Email,
            user.Role.ToString(),
            accessToken
        );
        return dto;
    }
}
