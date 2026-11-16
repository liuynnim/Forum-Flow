using ForumFlow.Api.Hubs;
using ForumFlow.Application;
using ForumFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── 0. NẠP FILE CẤU HÌNH LOCAL (CHỨA SECRETS CỦA MÁY CÁ NHÂN, ĐÃ BỊ GIT CHẶN) ──
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// ── 1. CORS ────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJsFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Bắt buộc khi dùng SignalR WebSocket
    });
});

// ── 2. Application & Infrastructure Services ───────────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── 3. SignalR ─────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── 4. Controllers & OpenAPI/Swagger ───────────────────────────────────────
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Chuẩn hóa lỗi Validation của Model Binding & [ApiController] theo đúng Spec Chapter-04
        options.InvalidModelStateResponseFactory = context =>
        {
            var details = context.ModelState
                .Where(ms => ms.Value?.Errors.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(err => new ForumFlow.Application.Common.Models.ValidationErrorDetail(
                    Field: ms.Key.TrimStart('$', '.').Replace("request.", ""),
                    Message: err.ErrorMessage
                )))
                .ToList();

            var envelope = ForumFlow.Application.Common.Models.ApiResponse<object>.Fail(
                code: "VALIDATION_ERROR",
                message: "Dữ liệu gửi lên không hợp lệ!",
                details: details
            );

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(envelope);
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowNextJsFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ── SignalR Hub Endpoints ──────────────────────────────────────────────────
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
