using ForumFlow.Domain.Common;

namespace ForumFlow.Domain.Entities;

/// <summary>
/// Ánh xạ bảng <c>categories</c> — danh mục chủ đề forum.
/// </summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;           // VARCHAR(100)
    public string Slug { get; set; } = string.Empty;           // VARCHAR(100) UNIQUE
    public string? Description { get; set; }
    public string? Icon { get; set; }                          // Icon name, e.g. 'code', 'chat'
    public string? Color { get; set; }                         // Hex color, e.g. '#3B82F6'
    public short DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
