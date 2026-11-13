using System.Text.RegularExpressions;
using ForumFlow.Application.Categories.Dtos;
using ForumFlow.Application.Common.Interfaces;
using ForumFlow.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ForumFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public CategoriesController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        var categories = await _context
            .Categories.Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.Icon,
                c.Color,
                c.DisplayOrder,
                c.Posts.Count(p => p.DeletedAt == null)
            ))
            .ToListAsync();
        return Ok(categories);
    }

    /// <summary>Tạo chuyên mục mới</summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Trường name ko thể toàn khoảng trắng" });
        }

        if (string.IsNullOrWhiteSpace(request.Slug))
        {
            return BadRequest(new { error = "Trường slug không được để trống!" });
        }

        string normalizedSlug = request.Slug.Trim().ToLowerInvariant();

        // Kiểm tra định dạng mã màu Hex (#RRGGBB) khớp với Check Constraint 'chk_categories_color' dưới PostgreSQL
        string? normalizedColor = string.IsNullOrWhiteSpace(request.Color)
            ? null
            : request.Color.Trim();

        if (normalizedColor is not null && !Regex.IsMatch(normalizedColor, @"^#[0-9A-Fa-f]{6}$"))
        {
            return BadRequest(
                new
                {
                    error = "Trường color phải đúng định dạng mã màu Hex 6 ký tự (ví dụ: '#4F46E5')!",
                }
            );
        }

        // kiểm tra trùng lặp slug
        bool slugExists = await _context.Categories.AnyAsync(c => c.Slug == normalizedSlug);
        if (slugExists)
        {
            return Conflict(new { error = $"Chuyên mục với slug '{normalizedSlug}' đã tồn tại!" });
        }
        Category category = new()
        {
            Name = request.Name.Trim(),
            Slug = normalizedSlug,
            Description = request.Description,
            Icon = request.Icon,
            Color = normalizedColor,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);
        CategoryDto resultDto = new(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.Icon,
            category.Color,
            category.DisplayOrder,
            0
        );

        return StatusCode(201, resultDto);
    }
}
