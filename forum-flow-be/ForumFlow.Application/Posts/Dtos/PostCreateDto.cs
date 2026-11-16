namespace ForumFlow.Application.Posts.Dtos;

public record CreatePostRequest(
    string Title,
    string Content,
    Guid CategoryId,
    List<string>? Tags = null
);
