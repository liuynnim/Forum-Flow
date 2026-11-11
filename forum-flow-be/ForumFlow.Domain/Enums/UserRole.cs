namespace ForumFlow.Domain.Enums;

/// <summary>
/// Phân quyền người dùng — ánh xạ PostgreSQL ENUM user_role
/// </summary>
public enum UserRole
{
    Member = 0,
    Moderator = 1,
    Admin = 2
}