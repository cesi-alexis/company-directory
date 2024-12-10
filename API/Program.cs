using CompanyDirectory.Common;
using CompanyDirectory.API.Contexts;
using CompanyDirectory.API.Middlewares;
using CompanyDirectory.API.Services;
using CompanyDirectory.API.Validation;
using Microsoft.EntityFrameworkCore;
using CompanyDirectory.API.Interfaces;
using CompanyDirectory.Models.Entities;

var builder = WebApplication.CreateBuilder(args);
// Configuration explicite pour Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.Configure(builder.Configuration.GetSection("Kestrel"));
    options.AddServerHeader = false;
});

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Ajouter le filtre globalement
    options.Filters.Add<ValidateModelAttribute>();
}).AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BusinessDirectoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddMemoryCache();
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));

builder.Services.AddScoped<ValidateModelAttribute>();
builder.Services.AddScoped<ICrudService<Location>, LocationService>();
builder.Services.AddScoped<ICrudService<Service>, ServiceService>();
builder.Services.AddScoped<IWorkerService, WorkerService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMiddleware<LoggingMiddleware>();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }