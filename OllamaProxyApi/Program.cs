using OllamaProxyApi.Services;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// ✅ 加入 CORS 設定（允許跨網域請求）
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://internal.hochi.org.tw:8083", "https://dict.hochi.org.tw:5263", "https://editor-bot.no8.io")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 加上這行：註冊 FlaskProxyService 所需的 HttpClient
builder.Services.AddHttpClient<FlaskProxyService>(client =>
{
    client.DefaultRequestHeaders.Accept.Clear(); // 建議加這行
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});


var app = builder.Build();

// Configure the HTTP request pipeline.

// ✅ 使用 CORS（一定要放在 UseAuthorization 之前）
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
