using Microsoft.EntityFrameworkCore;
using RegistrationPortal.Server.Data;
using RegistrationPortal.Server.Repositories;
using RegistrationPortal.Server.Services;
using RegistrationPortal.Server.Mapping;
using Mapster;
using RegistrationPortal.Server.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Mapster
MappingConfig.Configure();

// Add services to the container.
builder.Services.AddControllers();

// Configure CORS for Angular development server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure DbContext with Oracle
builder.Services.AddDbContext<RegistrationPortalDbContext>(options =>
    options.UseOracle(
        builder.Configuration.GetConnectionString("OracleConnection"),
        oracleOptions =>
        {
            //oracleOptions.UseOracleSQLCompatibility(OracleSQLCompatibility.);
        }));



// Register repositories
builder.Services.AddScoped<ICustMastRepository, CustMastRepository>();
builder.Services.AddScoped<IAccountMastRepository, AccountMastRepository>();
builder.Services.AddScoped<ICustomerDocumentRepository, CustomerDocumentRepository>();

// Register services
builder.Services.AddScoped<ICustMastService, CustMastService>();
builder.Services.AddScoped<IAccountMastService, AccountMastService>();
builder.Services.AddScoped<ICustomerDocumentService, CustomerDocumentService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add authorization services
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS policy
app.UseCors("AllowAngularApp");

// Use global exception handling middleware
app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
