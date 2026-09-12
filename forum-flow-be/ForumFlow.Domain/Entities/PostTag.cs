namespace ForumFlow.Domain.Entities;

/// <summary> 
/// Ánh xạ bảng `post_tags` — bảng junction N-N giữa Post và Tag.
/// </summary>
public class PostTag
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
