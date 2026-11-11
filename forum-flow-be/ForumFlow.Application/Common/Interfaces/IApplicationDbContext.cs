using ForumFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForumFlow.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Post> Posts { get; }
    DbSet<PostTag> PostTags { get; }
    DbSet<PostVote> PostVotes { get; }
    DbSet<Bookmark> Bookmarks { get; }
    DbSet<Comment> Comments { get; }
    DbSet<CommentVote> CommentVotes { get; }
    DbSet<PostEditHistory> PostEditHistories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

