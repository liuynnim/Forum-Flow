namespace ForumFlow.Domain.Entities;
/// <summary>
/// Ánh xạ bảng `bookmarks` — lưu bài viết yêu thích.
/// </summary>
public class Bookmark
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}