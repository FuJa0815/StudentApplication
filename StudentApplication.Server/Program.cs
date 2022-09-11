using Microsoft.EntityFrameworkCore;
using StudentApplication.Server.Attributes;
using StudentApplication.Server.Data;
using StudentApplication.Server.Hub;
using StudentApplication.Server.Services;
using StudentApplication.Server.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddLogging();

builder.Services.AddControllers(mvc =>
{
    mvc.Conventions.Add(new ControllerNameAttributeConvention());
    mvc.InputFormatters.Insert(0, MyJsonPatchInputFormatter.GetJsonPatchInputFormatter());
});
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddTransient(typeof(IRestService<,>), typeof(RestService<,>));

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

app.UseHttpsRedirection();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<NotificationHub>("/hub");

app.Run();