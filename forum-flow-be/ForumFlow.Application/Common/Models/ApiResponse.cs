namespace ForumFlow.Application.Common.Models;

/// <summary>
/// Chi tiết lỗi cho từng trường (field-level validation error)
/// </summary>
public record ValidationErrorDetail(
    string Field,
    string Message
);

/// <summary>
/// Đối tượng lỗi chuẩn theo Spec Forum-Flow Chapter-04
/// </summary>
public record ErrorResponse(
    string Code,
    string Message,
    List<ValidationErrorDetail>? Details = null
);

/// <summary>
/// Chuẩn Response Envelope cho toàn bộ hệ thống Forum-Flow
/// </summary>
public record ApiResponse<T>(
    bool Success,
    T? Data = default,
    string? Message = null,
    ErrorResponse? Error = null,
    object? Meta = null
)
{
    // Factory method cho kết quả thành công
    public static ApiResponse<T> Ok(T data, string message = "OK", object? meta = null)
        => new(true, data, message, null, meta);

    // Factory method cho kết quả thất bại
    public static ApiResponse<T> Fail(string code, string message, List<ValidationErrorDetail>? details = null)
        => new(false, default, null, new ErrorResponse(code, message, details));
}

