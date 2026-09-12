using ForumFlow.Domain.Common;
using ForumFlow.Domain.Enums;

namespace ForumFlow.Domain.Entities;

/// <summary>
/// Ánh xạ bảng <c>posts</c> — bài viết chính, trung tâm của Forum domain.
/// </summary>
public class Post : BaseEntity
{
    public string Title { get; set; } = string.Empty;          // VARCHAR(500), ≥10 ký tự
    public string Slug { get; set; } = string.Empty;           // VARCHAR(600) UNIQUE
    public string Content { get; set; } = string.Empty;        // TEXT — Markdown source
    public string? ContentHtml { get; set; }                   // TEXT — Rendered HTML (cached)
    public PostStatus Status { get; set; } = PostStatus.Published;
    public bool IsPinned { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public int VoteScore { get; set; } = 0;                    // Denormalized SUM(vote_type)
    public int CommentCount { get; set; } = 0;                 // Denormalized counter

    // Foreign Keys
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Navigation Properties
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostVote> PostVotes { get; set; } = new List<PostVote>();
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public ICollection<PostEditHistory> EditHistory { get; set; } = new List<PostEditHistory>();
}
