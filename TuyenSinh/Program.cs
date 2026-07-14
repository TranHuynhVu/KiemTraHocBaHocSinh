using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TuyenSinh.Models;
using TuyenSinh.Data;
using TuyenSinh.Services;
using Hangfire;


var builder = WebApplication.CreateBuilder(args);

// Add Connection String & DbContext
var connectionString = builder.Configuration.GetConnectionString("default");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Identity Services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure Cookie authentication options
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/tai-khoan/dang-nhap";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Register Business Services
builder.Services.AddScoped<IMonHocService, MonHocService>();
builder.Services.AddScoped<IToHopMonService, ToHopMonService>();
builder.Services.AddScoped<IHocBaService, HocBaService>();
builder.Services.AddScoped<INganhService, NganhService>();

// Configure Hangfire Services
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString));

builder.Services.AddHangfireServer();

builder.Services.AddMemoryCache();

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });



var app = builder.Build();

// Run database seeding on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    
    // Automatically create database and run migrations on remote server
    // await context.Database.MigrateAsync();

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await DbInitializer.SeedAsync(context, userManager, roleManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Process authentication cookies
app.UseAuthorization();  // Check authorization roles

app.UseHangfireDashboard();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

