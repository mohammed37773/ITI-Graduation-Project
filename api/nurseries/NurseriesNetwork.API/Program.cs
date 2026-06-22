<<<<<<< HEAD
=======
using NurseriesNetwork.Infrastructure.Extensions;
>>>>>>> main

namespace NurseriesNetwork.API
{
    public class Program
    {
<<<<<<< HEAD
        public static void Main(string[] args)
=======
        public static async Task Main(string[] args)
>>>>>>> main
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
<<<<<<< HEAD
=======
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddInfrastructure(builder.Configuration);
>>>>>>> main

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
<<<<<<< HEAD
                app.MapOpenApi();
            }

=======
                //app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    options.RoutePrefix = string.Empty;
                });
            }

            await app.SeedRolesAsync();

            app.UseAuthentication();
>>>>>>> main
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
