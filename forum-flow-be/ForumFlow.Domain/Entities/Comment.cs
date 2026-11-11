using ForumFlow.Domain.Common;

namespace ForumFlow.Domain.Entities;

/// <summary>
/// Ánh xạ bảng `comments` — bình luận lồng nhau dùng Adjacency List (tối đa 5 cấp).
/// </summary>
public class Comment : BaseEntity
{
    public string Content { get; set; } = string.Empty; // TEXT — Markdown
    public int VoteScore { get; set; } = 0; // Denormalized
    public short Depth { get; set; } = 0; // 0=root, 1=reply, max=5
    public int ReplyCount { get; set; } = 0; // Denormalized
    public bool IsDeleted { get; set; } = false;

    // Foreign Keys
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public Guid? ParentCommentId { get; set; } // NULL = root comment
    public Comment? ParentComment { get; set; }

    // Navigation Properties
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();
}
