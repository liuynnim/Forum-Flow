namespace ForumFlow.Domain.Entities;

/// <summary>
/// Ánh xạ bảng `comment_votes`.
/// </summary>
public class CommentVote
{
    public Guid CommentId { get; set; }
    public Comment Comment { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>1 = upvote, -1 = downvote</summary>
    public short VoteType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}