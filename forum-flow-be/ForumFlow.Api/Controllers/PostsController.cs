using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ForumFlow.Application.Common.Interfaces;
using ForumFlow.Application.Posts.Dtos;
using ForumFlow.Domain.Entities;
using ForumFlow.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ForumFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public PostsController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Lấy danh sách bài viết (phân trang), có thể lọc theo category</summary>
    [HttpGet]
    public async Task<ActionResult<List<PostSummaryDto>>> GetPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? categorySlug = null,
        [FromQuery] string sort = "newest"
    )
    {
        var query = _context.Posts.Where(p =>
            p.DeletedAt == null && p.Status == PostStatus.Published
        );

        if (!string.IsNullOrEmpty(categorySlug))
            query = query.Where(p => p.Category.Slug == categorySlug);

        query = sort switch
        {
            "hot" => query.OrderByDescending(p => p.VoteScore),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt),
        };

        var total = await query.CountAsync();
        var posts = await query
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostSummaryDto(
                p.Id,
                p.Title,
                p.Slug,
                p.Status,
                p.IsPinned,
                p.ViewCount,
                p.VoteScore,
                p.CommentCount,
                p.CreatedAt,
                new PostAuthorDto(
                    p.Author.Id,
                    p.Author.Username,
                    p.Author.DisplayName,
                    p.Author.AvatarUrl
                ),
                new PostCategoryDto(p.Category.Id, p.Category.Name, p.Category.Slug)
            ))
            .ToListAsync();

        return Ok(
            new
            {
                total,
                page,
                pageSize,
                data = posts,
            }
        );
    }

    /// <summary>Lấy chi tiết một bài viết theo slug</summary>
    [HttpGet("{slug}")]
    public async Task<ActionResult<PostDetailDto>> GetPostBySlug(string slug)
    {
        var post = await _context
            .Posts.Where(p => p.Slug == slug && p.DeletedAt == null)
            .Select(p => new PostDetailDto(
                p.Id,
                p.Title,
                p.Slug,
                p.Content,
                p.ContentHtml,
                p.Status,
                p.IsPinned,
                p.ViewCount,
                p.VoteScore,
                p.CommentCount,
                p.CreatedAt,
                p.UpdatedAt,
                new PostAuthorDto(
                    p.Author.Id,
                    p.Author.Username,
                    p.Author.DisplayName,
                    p.Author.AvatarUrl
                ),
                new PostCategoryDto(p.Category.Id, p.Category.Name, p.Category.Slug),
                p.PostTags.Select(pt => new PostTagDto(pt.Tag.Name, pt.Tag.Slug)).ToList()
            ))
            .FirstOrDefaultAsync();

        if (post is null)
            return NotFound();
        return Ok(post);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CreatePostRequest>> CreatePost(
        [FromBody] CreatePostRequest request
    )
    {
        if (!ModelState.IsValid || request is null)
        {
            return BadRequest(ModelState);
        }
        
        var userIdStr =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdStr, out Guid authorId))
            return Unauthorized(new { error = "Token không hợp lệ!" });

        using StreamReader reader = new(Request.Body);
        string body = await reader.ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(body))
            return BadRequest(new { error = "Body không được để trống!" });

        using JsonDocument doc = JsonDocument.Parse(body);
        JsonElement root = doc.RootElement;

        if (
            !root.TryGetProperty("title", out var titleProp)
            || string.IsNullOrWhiteSpace(titleProp.GetString())
        )
            return BadRequest(new { error = "Tiêu đề bài viết (title) bắt buộc nhập!" });

        string title = titleProp.GetString()!.Trim();
        if (title.Length < 10)
            return BadRequest(new { error = "Tiêu đề bài viết phải có ít nhất 10 ký tự!" });

        if (
            !root.TryGetProperty("content", out var contentProp)
            || string.IsNullOrWhiteSpace(contentProp.GetString())
        )
            return BadRequest(new { error = "Nội dung bài viết (content) bắt buộc nhập!" });

        string content = contentProp.GetString()!;

        if (
            !root.TryGetProperty("categoryId", out var catProp)
            || !Guid.TryParse(catProp.GetString(), out Guid categoryId)
        )
            return BadRequest(new { error = "categoryId bắt buộc và phải đúng định dạng GUID!" });
        bool categoryExists = await _context.Categories.AnyAsync(c =>
            c.Id == categoryId && c.IsActive
        );
        if (!categoryExists)
            return BadRequest(new { error = "Chuyên mục không tồn tại hoặc đã ngừng hoạt động!" });
        var tagNames = new List<string>();
        if (
            root.TryGetProperty("tags", out var tagsProp)
            && tagsProp.ValueKind != JsonValueKind.Null
        )
        {
            if (tagsProp.ValueKind != JsonValueKind.Array)
                return BadRequest(
                    new
                    {
                        error = $"Trường 'tags' yêu cầu kiểu Array, nhưng nhận được kiểu {tagsProp.ValueKind}!",
                    }
                );

            foreach (var item in tagsProp.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String)
                    return BadRequest(
                        new
                        {
                            error = $"Mỗi phần tử trong 'tags' phải là kiểu String, nhưng nhận được kiểu {item.ValueKind}!",
                        }
                    );

                var t = item.GetString()?.Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(t) && !tagNames.Contains(t))
                    tagNames.Add(t);
            }
        }
        var post = new Post
        {
            Title = title,
            Slug = GenerateSlug(title),
            Content = content,
            AuthorId = authorId,
            CategoryId = categoryId,
            Status = PostStatus.Published,
        };
        foreach (var tagName in tagNames)
        {
            var tagSlug = GenerateSlug(tagName, appendRandomSuffix: false);
            var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Slug == tagSlug);

            if (existingTag is null)
            {
                existingTag = new Tag
                {
                    Name = tagName,
                    Slug = tagSlug,
                    PostCount = 1,
                };
                _context.Tags.Add(existingTag);
            }
            else
            {
                existingTag.PostCount += 1;
            }

            post.PostTags.Add(new PostTag { PostId = post.Id, Tag = existingTag });
        }

        _context.Posts.Add(post);
        await _context.SaveChangesAsync(CancellationToken.None);

        return StatusCode(
            201,
            new
            {
                post.Id,
                post.Title,
                post.Slug,
                post.CategoryId,
                post.AuthorId,
                post.CreatedAt,
            }
        );
    }

    private static string GenerateSlug(string phrase, bool appendRandomSuffix = true)
    {
        string str = phrase.ToLowerInvariant().Replace("đ", "d");
        str = str.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char c in str)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        str = sb.ToString().Normalize(NormalizationForm.FormC);
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", "-").Trim('-');
        str = Regex.Replace(str, @"-+", "-");

        if (appendRandomSuffix)
        {
            string suffix = Guid.NewGuid().ToString("N")[..6];
            return $"{str}-{suffix}";
        }

        return str;
    }
}
