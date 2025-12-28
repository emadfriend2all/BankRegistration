using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using RegistrationPortal.Server.Data;
using RegistrationPortal.Server.Repositories;
using RegistrationPortal.Server.Services;
using RegistrationPortal.Server.Mapping;
using RegistrationPortal.Server.Entities;
using RegistrationPortal.Server.Entities.Identity;
using Mapster;
using RegistrationPortal.Server.Middleware;
using RegistrationPortal.Server.Constants;
using RegistrationPortal.Server.Attributes;

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
        policy.WithOrigins("http://localhost:4200", "http://localhost:52332")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure DbContext with Oracle for Oracle 11g compatibility
builder.Services.AddDbContext<RegistrationPortalDbContext>(options =>
    options.UseOracle(
        builder.Configuration.GetConnectionString("OracleConnection"),
        oracleOptions =>
        {
            // Oracle 11g doesn't have specific compatibility enum, so we configure manually
            // oracleOptions.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
            
            // Disable features not supported in Oracle 11g
            oracleOptions.UseRelationalNulls();
            
            // Configure command timeout for better performance with older Oracle versions
            oracleOptions.CommandTimeout(30);
        }));



// Register repositories
builder.Services.AddScoped<ICustMastRepository, CustMastRepository>();
builder.Services.AddScoped<IAccountMastRepository, AccountMastRepository>();
builder.Services.AddScoped<ICustomerDocumentRepository, CustomerDocumentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// Register services
builder.Services.AddScoped<ICustMastService, CustMastService>();
builder.Services.AddScoped<IAccountMastService, AccountMastService>();
builder.Services.AddScoped<ICustomerDocumentService, CustomerDocumentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Register JWT and Authentication services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register password hasher for seeding
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Register seeder service
builder.Services.AddScoped<ISeederService, SeederService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

// Configure JWT options for the custom handler
builder.Services.Configure<JwtBearerOptions>(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "RegistrationPortal",
        ValidAudience = jwtSettings["Audience"] ?? "RegistrationPortalUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "TokenOnly";
    options.DefaultChallengeScheme = "TokenOnly";
})
.AddScheme<AuthenticationSchemeOptions, TokenOnlyAuthenticationHandler>("TokenOnly", null);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RegistrationPortal API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Token", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Token",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token (without 'Bearer ' prefix)"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Token"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register permission authorization handler
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Add authorization services with permission-based policies
builder.Services.AddAuthorization(options =>
{
    // Customer permissions
    options.AddPolicy(Permissions.Customers.Create, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Customers.Create)));
    options.AddPolicy(Permissions.Customers.Update, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Customers.Update)));
    options.AddPolicy(Permissions.Customers.Delete, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Customers.Delete)));
    options.AddPolicy(Permissions.Customers.List, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Customers.List)));
    options.AddPolicy(Permissions.Customers.ViewDetails, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Customers.ViewDetails)));
    options.AddPolicy(Permissions.Customers.Approve, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Customers.Approve)));
    options.AddPolicy(Permissions.Customers.Reject, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Customers.Reject)));

    // Account permissions
    options.AddPolicy(Permissions.Accounts.Create, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Accounts.Create)));
    options.AddPolicy(Permissions.Accounts.Update, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Accounts.Update)));
    options.AddPolicy(Permissions.Accounts.Delete, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Accounts.Delete)));
    options.AddPolicy(Permissions.Accounts.List, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Accounts.List)));

    // User permissions
    options.AddPolicy(Permissions.Users.Create, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Users.Create)));
    options.AddPolicy(Permissions.Users.Update, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Users.Update)));
    
    options.AddPolicy(Permissions.Users.List, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Users.List)));
    options.AddPolicy(Permissions.Users.ViewDetails, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Users.ViewDetails)));
    
    options.AddPolicy(Permissions.Users.AssignRoles, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Users.AssignRoles)));

    // Role permissions
    options.AddPolicy(Permissions.Roles.Create, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Roles.Create)));
    options.AddPolicy(Permissions.Roles.Update, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Roles.Update)));
    options.AddPolicy(Permissions.Roles.Delete, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Roles.Delete)));
    options.AddPolicy(Permissions.Roles.List, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Roles.List)));
    options.AddPolicy(Permissions.Roles.ViewDetails, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Roles.ViewDetails)));
    options.AddPolicy(Permissions.Roles.AssignPermissions, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Roles.AssignPermissions)));

    // System permissions    
    options.AddPolicy(Permissions.System.Maintenance, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.System.Maintenance)));

    // Dashboard permissions
    options.AddPolicy(Permissions.Dashboard.View, policy => 
        policy.Requirements.Add(new PermissionRequirement(Permissions.Dashboard.View)));
});

var app = builder.Build();

// Ensure database is created and run migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RegistrationPortalDbContext>();
    try
    {
        // For Oracle, we need to check if database exists first
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            Console.WriteLine("Database does not exist. Creating new database...");
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine("Database created successfully");
        }
        else
        {
            Console.WriteLine("Database already exists and is accessible");
        }
        
        // Run pending migrations with better error handling
        Console.WriteLine("Running database migrations...");
        try
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("Migrations completed successfully");
        }
        catch (Exception migrationEx)
        {
            Console.WriteLine($"Migration warning: {migrationEx.Message}");
            Console.WriteLine("This might be expected if tables already exist");
            // Continue anyway - the application might still work
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database operation failed: {ex.Message}");
        if (ex.Message.Contains("ORA-00955") || ex.Message.Contains("already exists"))
        {
            Console.WriteLine("Object already exists - this is expected for existing databases");
        }
        else
        {
            Console.WriteLine("Continuing with application startup...");
        }
        // Continue anyway - let the application handle database issues at runtime
    }
}

// Seed data on application startup
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<ISeederService>();
    await seeder.SeedDataAsync();
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RegistrationPortal API V1");
        c.RoutePrefix = "swagger";
    });
//}

// Use CORS policy
app.UseCors("AllowAngularApp");

// Use global exception handling middleware
app.UseGlobalExceptionHandling();

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Fallback to Angular index.html for SPA routing
app.MapFallbackToFile("/index.html");

app.Run();
