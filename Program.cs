using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// 註冊服務
builder.Services.AddControllers();

// 註冊 IHttpClientFactory
builder.Services.AddHttpClient();

// 註冊日誌記錄器
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders(); // 清除默認的日誌提供程序
    logging.AddConsole(); // 添加控制台日誌
    // 如果需要寫入到文件，請配置 Serilog 或其他庫
});

// 設置 ApiSettings 配置來從 appsettings.json 中加載配置
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// 構建應用
var app = builder.Build();

// 配置 HTTP 請求管道
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 映射控制器
app.MapControllers();

app.Run();

// 定義 ApiSettings 類來映射配置
public class ApiSettings
{
    public string ApiKey { get; set; }
}
