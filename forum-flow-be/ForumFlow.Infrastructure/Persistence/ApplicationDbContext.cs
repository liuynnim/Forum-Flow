using ForumFlow.Application.Common.Interfaces;
using ForumFlow.Domain.Entities;
using ForumFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ForumFlow.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // DbSets — Forum Domain
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<PostVote> PostVotes => Set<PostVote>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentVote> CommentVotes => Set<CommentVote>();
    public DbSet<PostEditHistory> PostEditHistories => Set<PostEditHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── users ──────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            // Unique indexes
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();

            // Column lengths — ánh xạ VARCHAR theo spec
            entity.Property(u => u.Username).HasMaxLength(50).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(500); // nullable (OAuth)
            entity.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.AvatarUrl).HasMaxLength(500);
            entity.Property(u => u.WebsiteUrl).HasMaxLength(255);
            entity.Property(u => u.GithubUrl).HasMaxLength(255);
            entity.Property(u => u.Bio).HasColumnType("text"); // TEXT — không giới hạn
            entity.Property(u => u.BanReason).HasColumnType("text");
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

            // Check constraints
            entity.ToTable(t =>
                t.HasCheckConstraint("chk_users_username_length", "LENGTH(\"Username\") >= 3")
            );

            entity.ToTable(t =>
                t.HasCheckConstraint(
                    "chk_users_ban_logic",
                    "\"IsBanned\" = FALSE OR \"BanReason\" IS NOT NULL"
                )
            );
        });

        // ── categories ─────────────────────────────────────────────────────
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Slug).IsUnique();

            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Slug).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Description).HasColumnType("text");
            entity.Property(c => c.Icon).HasMaxLength(50);
            entity.Property(c => c.Color).HasMaxLength(7); // '#3B82F6'

            // Check: color phải đúng định dạng hex
            entity.ToTable(t =>
                t.HasCheckConstraint(
                    "chk_categories_color",
                    "\"Color\" IS NULL OR \"Color\" ~ '^#[0-9A-Fa-f]{6}$'"
                )
            );
        });

        // ── tags ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(t => t.Name).IsUnique();
            entity.HasIndex(t => t.Slug).IsUnique();

            entity.Property(t => t.Name).HasMaxLength(50).IsRequired();
            entity.Property(t => t.Slug).HasMaxLength(50).IsRequired();
            entity.Property(t => t.Description).HasColumnType("text");
        });

        // ── posts ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasIndex(p => p.Slug).IsUnique();

            entity.Property(p => p.Title).HasMaxLength(500).IsRequired();
            entity.Property(p => p.Slug).HasMaxLength(600).IsRequired();
            entity.Property(p => p.Content).HasColumnType("text").IsRequired();
            entity.Property(p => p.ContentHtml).HasColumnType("text");
            entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

            // Check: title ≥ 10 ký tự
            entity.ToTable(t =>
                t.HasCheckConstraint("chk_posts_title_length", "LENGTH(\"Title\") >= 10")
            );

            entity
                .HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity
                .HasOne(p => p.Category)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── post_tags (composite PK) ───────────────────────────────────────
        modelBuilder.Entity<PostTag>(entity =>
        {
            entity.HasKey(pt => new { pt.PostId, pt.TagId });
            entity
                .HasOne(pt => pt.Post)
                .WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── post_votes (composite PK) ──────────────────────────────────────
        modelBuilder.Entity<PostVote>(entity =>
        {
            entity.HasKey(pv => new { pv.PostId, pv.UserId });

            // Check: vote_type chỉ được là 1 (upvote) hoặc -1 (downvote)
            entity.ToTable(t =>
                t.HasCheckConstraint("chk_post_votes_type", "\"VoteType\" IN (1, -1)")
            );

            entity
                .HasOne(pv => pv.Post)
                .WithMany(p => p.PostVotes)
                .HasForeignKey(pv => pv.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(pv => pv.User)
                .WithMany(u => u.PostVotes)
                .HasForeignKey(pv => pv.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── bookmarks (composite PK) ───────────────────────────────────────
        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(b => new { b.UserId, b.PostId });
            entity
                .HasOne(b => b.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(b => b.Post)
                .WithMany(p => p.Bookmarks)
                .HasForeignKey(b => b.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── comments ───────────────────────────────────────────────────────
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.Property(c => c.Content).HasColumnType("text").IsRequired();

            // Check: tối đa 5 cấp lồng nhau
            entity.ToTable(t => t.HasCheckConstraint("chk_comments_depth", "\"Depth\" <= 5"));

            // Check: content không được rỗng/chỉ toàn khoảng trắng
            entity.ToTable(t =>
                t.HasCheckConstraint("chk_comments_content", "LENGTH(TRIM(\"Content\")) > 0")
            );

            entity
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── comment_votes (composite PK) ───────────────────────────────────
        modelBuilder.Entity<CommentVote>(entity =>
        {
            entity.HasKey(cv => new { cv.CommentId, cv.UserId });

            // Check: vote_type chỉ được là 1 hoặc -1
            entity.ToTable(t =>
                t.HasCheckConstraint("chk_comment_votes_type", "\"VoteType\" IN (1, -1)")
            );

            entity
                .HasOne(cv => cv.Comment)
                .WithMany(c => c.CommentVotes)
                .HasForeignKey(cv => cv.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(cv => cv.User)
                .WithMany(u => u.CommentVotes)
                .HasForeignKey(cv => cv.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── post_edit_history ──────────────────────────────────────────────
        modelBuilder.Entity<PostEditHistory>(entity =>
        {
            entity.Property(h => h.PreviousTitle).HasMaxLength(500).IsRequired();
            entity.Property(h => h.PreviousContent).HasColumnType("text").IsRequired();
            entity.Property(h => h.EditReason).HasMaxLength(255);

            entity
                .HasOne(h => h.Post)
                .WithMany(p => p.EditHistory)
                .HasForeignKey(h => h.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(h => h.Editor)
                .WithMany()
                .HasForeignKey(h => h.EditorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
