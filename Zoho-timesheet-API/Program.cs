using Microsoft.EntityFrameworkCore;
using Serilog;
using Zoho_timesheet_API.Services;
using Zoho_timesheet_EFC;


// Configure Serilog
Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.File(@"C:/inetpub/TimeTrackerApp/logs/log-.txt", rollingInterval: RollingInterval.Day).CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpClient();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddDbContext<ZohoTimesheetDBContext>(options =>
                     options.UseSqlServer(
                    builder.Configuration["ConnectionStrings:DefaultConnection"]
               ));

builder.Services.AddScoped<CommonService>();
builder.Services.AddScoped<ISesSmtpEmailService, SesSmtpEmailService>();


builder.Services.AddLogging();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ZohoTimesheetDBContext>();
    context.ApplyMigrations();
}
app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();