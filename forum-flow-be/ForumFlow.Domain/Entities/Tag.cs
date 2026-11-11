using ForumFlow.Domain.Common;

namespace ForumFlow.Domain.Entities;

/// <summary>
/// Ánh xạ bảng <c>tags</c> — nhãn gắn vào bài viết.
/// </summary>
public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;           // VARCHAR(50) UNIQUE
    public string Slug { get; set; } = string.Empty;           // VARCHAR(50) UNIQUE
    public string? Description { get; set; }
    public int PostCount { get; set; } = 0;                    // Denormalized counter

    // Navigation Properties
    public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}
