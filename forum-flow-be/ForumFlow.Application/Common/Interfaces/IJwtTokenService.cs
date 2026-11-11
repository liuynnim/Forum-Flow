using ForumFlow.Domain.Entities;

namespace ForumFlow.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
