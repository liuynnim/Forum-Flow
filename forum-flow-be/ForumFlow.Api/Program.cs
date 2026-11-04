using ForumFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── 1. ĐĂNG KÝ SERVICES (DI CONTAINER) ──────────────────────
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// ── 2. ĐÓNG GÓI VÀ KHÓA CONTAINER ───────────────────────────
var app = builder.Build();

// ── 3. THIẾT LẬP MIDDLEWARE PIPELINE ─────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
