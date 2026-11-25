using HRM.Api;
using HRM.Api.Filters;
using HRM.Api.Middleware;
using Microsoft.AspNetCore.ResponseCompression;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);

builder.Services.AddScoped<RequireSmartEosUserFilter>();

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<RequireSmartEosUserFilter>();
    })
    .AddNewtonsoftJson(options =>
    {
        // Giữ nguyên PascalCase giống ASP.NET Web API 2 để tương thích 100% Frontend cũ
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("HrmAllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHrmServices(builder.Configuration);

var app = builder.Build();

HostingEnvironmentCompat.Configure(
    app.Environment.ContentRootPath,
    builder.Configuration["HrmSettings:LegacyTemplateRoot"] ?? @"e:\HRM\backend\HRMNew",
    builder.Configuration["HrmSettings:PathUploads"],
    builder.Configuration["HrmSettings:PathUploadsGoc"] ?? builder.Configuration["HrmSettings:PathUploads"]);

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("HrmAllowAll");

app.UseMiddleware<SmartEosAuthenticationMiddleware>();

app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "ActionApi",
    pattern: "api/{controller}/{action}/{id?}");
app.MapControllerRoute(
    name: "DefaultApi",
    pattern: "api/{controller}/{id?}");

app.Run();
