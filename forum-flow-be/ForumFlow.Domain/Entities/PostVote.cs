namespace ForumFlow.Domain.Entities;

/// <summary>
/// Ánh xạ bảng `post_votes` — upvote/downvote bài viết.
/// </summary>
public class PostVote
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>1 = upvote, -1 = downvote</summary>
    public short VoteType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
