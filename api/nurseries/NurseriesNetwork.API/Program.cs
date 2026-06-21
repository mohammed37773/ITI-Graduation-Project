using Microsoft.AspNetCore.Identity;
using NurseriesNetwork.AI.Services;
using NurseriesNetwork.API.Extensions;
using NurseriesNetwork.API.Middlewares;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Interfaces.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddCorsPolicy();
builder.Services.AddSwaggerWithJwt();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient<GeminiService>();
builder.Services.AddScoped<RagService>();
builder.Services.AddScoped<IAiService, RecommendationService>();
var app = builder.Build();

// Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Angular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed Roles & Admin
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    // إنشاء الـ Roles
    string[] roles = ["Admin", "Parent", "NurseryAdmin"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // إنشاء الـ Admin
    var adminEmail = "admin@nurseries.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            FullName = "Super Admin",
            Email = adminEmail,
            UserName = adminEmail,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(admin, "Admin@123456");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}

app.Run();