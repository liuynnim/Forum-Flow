namespace ForumFlow.Application.Categories.Dtos;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? Icon,
    string? Color,
    short DisplayOrder,
    int PostCount
);

public record CreateCategoryRequest(
    string Name,
    string Slug,
    string? Description,
    string? Icon,
    string? Color,
    short DisplayOrder = 0
);
