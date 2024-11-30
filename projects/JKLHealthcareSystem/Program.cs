using DotNetEnv;
using JKLHealthcareSystem.Data;
using JKLHealthcareSystem.Hubs;
using Microsoft.EntityFrameworkCore;
using JKLHealthcareSystem.Helpers;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
Env.Load();

// Get the connection string from environment variables
var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

if (string.IsNullOrEmpty(dbConnectionString))
{
    throw new InvalidOperationException("Database connection string is not set.");
}

// Configure services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(dbConnectionString));
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SessionHelper>(); 

var app = builder.Build();

// Developer exception page in development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Middleware pipeline
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Redirect unauthenticated users to the Login page
app.Use(async (context, next) =>
{
    var isLoggedIn = context.Session.GetString("isLoggedIn") == "true";
    if (context.Request.Path.StartsWithSegments("/Patients") && !isLoggedIn)
    {
        context.Response.Redirect("/Auth/Login");
        return;
    }

    if (context.Request.Path.StartsWithSegments("/Caregivers") && !isLoggedIn)
    {
        context.Response.Redirect("/Auth/Login");
        return;
    }

    if (context.Request.Path.StartsWithSegments("/Appointments") && !isLoggedIn)
    {
        context.Response.Redirect("/Auth/Login");
        return;
    }

    if (context.Request.Path == "/Auth/Logout")
    {
        context.Response.Redirect("/Auth/Login");
        return;
    }

    if (context.Request.Path == "/" && !isLoggedIn)
    {
        context.Response.Redirect("/Auth/Login");
        return;
    }

    if (context.Request.Path == "/" && isLoggedIn)
    {
        context.Response.Redirect("/Patients/Index");
        return;
    }

    await next();
});

app.UseAuthorization();

// Set the default route to the Login page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run("http://0.0.0.0:5088");
