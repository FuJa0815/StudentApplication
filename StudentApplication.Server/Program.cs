using Microsoft.EntityFrameworkCore;
using StudentApplication.Server.Attributes;
using StudentApplication.Server.Data;
using StudentApplication.Server.Hub;
using StudentApplication.Server.Services;

namespace StudentApplication.Server;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors();
        
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddLogging();

        builder.Services.AddControllers(mvc =>
        {
            mvc.Conventions.Add(new RestEndpointAttributeConvention());
        }).AddNewtonsoftJson();
        builder.Services.AddRouting(options => options.LowercaseUrls = true);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSignalR();

        builder.Services.AddTransient(typeof(IRestService<,>), typeof(RestService<,>));
        builder.Services.AddScoped<ICoursesService, CoursesService>();

        var app = builder.Build();

// Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
    
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseCors(options => options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

        //app.UseHttpsRedirection();

        app.UseRouting();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapHub<NotificationHub>("/hub");

        await app.RunAsync();
    }
}