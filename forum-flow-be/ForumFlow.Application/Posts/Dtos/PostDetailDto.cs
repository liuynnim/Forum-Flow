using ForumFlow.Domain.Enums;

namespace ForumFlow.Application.Posts.Dtos;

public record PostDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string Content,
    string? ContentHtml,
    PostStatus Status,
    bool IsPinned,
    int ViewCount,
    int VoteScore,
    int CommentCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    PostAuthorDto Author,
    PostCategoryDto Category,
    List<PostTagDto> Tags
);

public record PostTagDto(string Name, string Slug);
