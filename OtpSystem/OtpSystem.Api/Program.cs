using System.Data;
using System.Text;
using FluentValidation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OtpSystem.Api;
using OtpSystem.Application.Services;
using OtpSystem.Application.Validators;
using OtpSystem.Domain.Interfaces;
using OtpSystem.Infrastructure.Persistence;
using OtpSystem.Infrastructure.Persistence.Repositories;
using OtpSystem.Infrastructure.Persistence.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Adding Repositories, Services and the DbFactory...all from the Infrastructure Project

builder.Services.AddSingleton<DbConnectionFactory>();

builder.Services.AddScoped<IOtpRepository, OtpRepositories>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<OtpServices>();

builder.Services.AddScoped<IEmailServices>(_ =>
{
    var settings = _.GetRequiredService<IOptions<SmtpSettings>>().Value;
    return new SmtpEmailService(settings);
});

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

//Ends here...

builder.Services.AddValidatorsFromAssemblyContaining<GenerateOtpRequestValidator>();

builder
    .Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!)
            ),
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

//This is to test my connection to the database
app.MapGet(
    "/health/db",
    (DbConnectionFactory factory) =>
    {
        using var conn = factory.CreateConnection();
        conn.Open();
        return Results.Ok("DB Connected");
    }
);

app.MapOtpEndPoints();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

// app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching",
};

app.MapGet(
        "/weatherforecast",
        () =>
        {
            var forecast = Enumerable
                .Range(1, 5)
                .Select(index => new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        }
    )
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
