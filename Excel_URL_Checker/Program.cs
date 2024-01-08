using Excel_URL_Checker.Interfaces;
using Excel_URL_Checker.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDatasourceService, DatasourceService>();
builder.Services.AddScoped<ICompareService, CompareService>();
builder.Services.AddScoped<ICreateExcelService, CreateExcelService>();
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = long.MaxValue;
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // Optional: Enable compression for HTTPS requests
    options.Providers.Add<GzipCompressionProvider>(); // Enable GZIP compression
    // options.Providers.Add<DeflateCompressionProvider>(); // Enable Deflate compression (optional)
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest; // Set the compression level (e.g., Fastest, Optimal, NoCompression)
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.AllowSynchronousIO = true;
});


var app = builder.Build();

app.UseCors(opt =>
{
    opt.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseResponseCompression();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
