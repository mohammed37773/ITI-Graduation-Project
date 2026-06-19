using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NurseriesNetwork.Core.Entities ;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Core.Interfaces.Services;
using NurseriesNetwork.Infrastructure.Data;
using NurseriesNetwork.Infrastructure.Repositories;
using NurseriesNetwork.Infrastructure.Services;
using NurseriesNetwork.Infrastructure.Services.Payment;
using NurseriesNetwork.AI.Services;

namespace NurseriesNetwork.API.Extensions;

public static class ServiceExtensions
{
    // Database
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                config.GetConnectionString("DefaultConnection")));
        return services;
    }

    // Identity
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    // JWT
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme =
                JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["JWT:Issuer"],
                ValidAudience = config["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(config["JWT:Key"]!))
            };
        });

        return services;
    }

    // Repositories & Services
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IAiService, RecommendationService>();

        // Payment Strategy Pattern
        //services.AddScoped<VodafoneCashService>();
        //services.AddScoped<MezaService>();
        services.AddHttpClient<PaymobService>();
        services.AddHttpClient<PayPalService>();
        services.AddScoped<PaymobService>();
        services.AddScoped<PayPalService>();
        services.AddScoped<IPaymentFactory, PaymentFactory>();
        services.AddScoped<IPaymentGatewayService>(sp =>
            sp.GetRequiredService<PaymobService>());

       // services.AddScoped<PayPalService>();
       // services.AddScoped<IPaymentFactory, PaymentFactory>();

        return services;
    }

    // CORS
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("Angular", policy =>
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });

        return services;
    }
}
