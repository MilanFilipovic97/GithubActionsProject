using FluentValidation;
using FluentValidation.AspNetCore;
using IdentityApi.Database;
using IdentityApi.Interfaces;
using IdentityApi.MappingProfiles;
using IdentityApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddAutoMapper(typeof(CompanyProfile));

// Register FluentValidation
/*builder.Services.AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<CompanyValidator>();*/

builder.Services.AddFluentValidationAutoValidation()
    .AddValidatorsFromAssembly(typeof(Program).Assembly);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

string[] allowedOrigins = ["*"];

app.UseCors(x => x.AllowAnyHeader()
    .AllowAnyMethod()
    .WithOrigins(allowedOrigins));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }  // You need this line for integration tests