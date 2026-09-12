namespace ForumFlow.Domain.Enums;

/// <summary>
/// Trạng thái bài viết — ánh xạ PostgreSQL ENUM post_status
/// </summary>
public enum PostStatus
{
    Draft = 0,
    Published = 1,
    Locked = 2,
    Deleted = 3
}
