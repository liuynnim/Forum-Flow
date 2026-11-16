using ForumFlow.Domain.Enums;

namespace ForumFlow.Application.Posts.Dtos;

public record PostSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    PostStatus Status,
    bool IsPinned,
    int ViewCount,
    int VoteScore,
    int CommentCount,
    DateTime CreatedAt,
    PostAuthorDto Author,
    PostCategoryDto Category
);

public record PostAuthorDto(Guid Id, string Username, string DisplayName, string? AvatarUrl);

public record PostCategoryDto(Guid Id, string Name, string Slug);
