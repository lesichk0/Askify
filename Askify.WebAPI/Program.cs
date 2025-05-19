using System.Security.Claims;
using System.Text;
using Askify.BusinessLogicLayer.Configurations;
using Askify.BusinessLogicLayer.Interfaces;
using Askify.BusinessLogicLayer.Services;
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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAutoMapper(typeof(MappingProfile));

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    
    // Add event handlers for debugging token validation issues
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token successfully validated");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine($"Token received: {context.Token?.Substring(0, Math.Min(20, context.Token?.Length ?? 0))}...");
            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
        ClockSkew = TimeSpan.Zero, // No tolerance for token expiration
        NameClaimType = ClaimTypes.NameIdentifier // Ensure correct claim type for User.Identity.Name
    };
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

// Add or update CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:3000", "http://localhost:8080")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
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

// Use CORS before routing
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

// Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Run the app
app.Run();
