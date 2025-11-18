using FastPMS.Data;
using FastPMS.Hubs;
using FastPMS.ImageRepository;
using FastPMS.Models.Domain;
using FastPMS.Repositories;
using FastPMS.Repositories.Interfaces;
using FastPMS.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddDbContext<PmsDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("PmsDbConnectionString")));

builder.Services.AddIdentity<Users, IdentityRole>(Options =>
{
    Options.Password.RequireDigit = true;
    Options.Password.RequireLowercase = true;
    Options.Password.RequiredLength = 6;
    Options.User.RequireUniqueEmail = true;
    Options.SignIn.RequireConfirmedEmail = false;
    Options.SignIn.RequireConfirmedPhoneNumber = false;
    Options.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<PmsDbContext>().AddDefaultTokenProviders();

// dependency for Repository 
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IDeveloperRepository, DeveloperRepository>();
builder.Services.AddScoped<IImageRepo, CloudinaryImageRepo>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();

// ==================== AI ASSISTANT SERVICES ====================
// Add HttpClient for DeepSeek API
//builder.Services.AddHttpClient();

// Add AI Services
//builder.Services.AddScoped<IDeepSeekService, DeepSeekService>();
// ==================== END AI ASSISTANT SERVICES ====================

var app = builder.Build();

await SeedService.SeedDatabase(app.Services);

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
app.UseAuthentication();

app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();