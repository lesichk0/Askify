using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt; // Add this import for JwtSecurityToken
using Askify.BusinessLogicLayer.Configurations;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.BusinessLogicLayer.Services;
using Askify.BusinessLogicLayer.Options; // Add this import
using Askify.DataAccessLayer;
using Askify.DataAccessLayer.Data;
using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces;
using Askify.DataAccessLayer.Seeding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Askify.BusinessLogicLayer.Validators;
using Askify.WebAPI.Filters;
using Askify.WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationExceptionFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    // This maintains the default behavior
    options.SuppressModelStateInvalidFilter = false;
});
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with OAuth2 support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Askify API", 
        Version = "v1",
        Description = "API for Askify - Q&A and Expert Consultation Platform"
    });
    
    // Include JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions => sqlOptions.EnableRetryOnFailure()
                           .CommandTimeout(30)));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAutoMapper(typeof(MappingProfile));

// Ensure JWT settings are loaded correctly
var jwtSettingsSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(jwtSettingsSection);

var jwtSettings = jwtSettingsSection.Get<JwtOptions>() ?? 
    throw new InvalidOperationException("JWT settings section not found or empty.");

// Validate JWT settings at startup time
if (string.IsNullOrEmpty(jwtSettings.Key))
    throw new InvalidOperationException("JWT Key is not configured in appsettings.json");
if (string.IsNullOrEmpty(jwtSettings.Issuer))
    throw new InvalidOperationException("JWT Issuer is not configured in appsettings.json");
if (string.IsNullOrEmpty(jwtSettings.Audience))
    throw new InvalidOperationException("JWT Audience is not configured in appsettings.json");

// JWT authentication setup
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Add this debugging section to help troubleshoot token issues
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            Console.WriteLine($"JWT Token received: {context.Token}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully");
            return Task.CompletedTask;
        }
    };
      options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,               // Temporarily disable for troubleshooting
        ValidateAudience = false,             // Temporarily disable for troubleshooting
        ValidateLifetime = true,              // Keep validating expiration
        ValidateIssuerSigningKey = true,      // We need this to validate the signature
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ClockSkew = TimeSpan.FromHours(1),    // Increased to 1 hour to handle time differences
        RequireSignedTokens = true,
        RequireExpirationTime = true,
        ValidateActor = false,
        ValidateTokenReplay = false,
        // Remove duplicate RequireSignedTokens setting
        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
    };
    
    options.SaveToken = true;
    
    // Print more detailed token debugging info
    Console.WriteLine($"JWT Configuration - Issuer: {jwtSettings.Issuer}, Audience: {jwtSettings.Audience}");
    Console.WriteLine($"Full JWT Key: {jwtSettings.Key}");
});

builder.Services.AddAuthorization();

// Repositories and UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Seeders
builder.Services.AddScoped<RoleSeeder>();
builder.Services.AddScoped<UserSeeder>();
builder.Services.AddScoped<ConsultationSeeder>();
builder.Services.AddScoped<TagSeeder>();
builder.Services.AddScoped<PostSeeder>();
builder.Services.AddScoped<MessageSeeder>();
builder.Services.AddScoped<FeedbackSeeder>();
builder.Services.AddScoped<SubscriptionSeeder>();
builder.Services.AddScoped<PaymentSeeder>();
builder.Services.AddScoped<ReportSeeder>();
builder.Services.AddScoped<NotificationSeeder>();
builder.Services.AddScoped<CommentSeeder>();
builder.Services.AddScoped<CommentLikeSeeder>();
builder.Services.AddScoped<PostLikeSeeder>();
builder.Services.AddScoped<SavedPostSeeder>();
builder.Services.AddScoped<DataSeeder>();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserDtoValidator>();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add the error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

// Use CORS before routing
app.UseCors();

// Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Rebuild model cache and fix database schema
app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Checking database schema and adding missing columns if needed");
        
        // Force new connection
        Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
        
        // Execute direct SQL to add columns
        await context.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                          WHERE TABLE_NAME = 'Consultations' AND COLUMN_NAME = 'Title')
            BEGIN
                ALTER TABLE Consultations ADD Title nvarchar(max) NOT NULL DEFAULT 'Untitled Consultation'
            END

            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                          WHERE TABLE_NAME = 'Consultations' AND COLUMN_NAME = 'Description')
            BEGIN
                ALTER TABLE Consultations ADD Description nvarchar(max) NOT NULL DEFAULT 'No description provided.'
            END");
        
        logger.LogInformation("Database schema check completed");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error checking/updating database schema");
    }
});

// Force EF Core to rebuild its model cache
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Clear connection pools to force new connections
        Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
        
        // Open and close connection to refresh schema
        db.Database.OpenConnection();
        db.Database.CloseConnection();
        
        app.Logger.LogInformation("Database connection reset performed to refresh schema cache");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while refreshing database schema");
    }
}

// Run the app
app.Run();
