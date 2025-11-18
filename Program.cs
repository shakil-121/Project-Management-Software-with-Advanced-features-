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

// ✅ SIGNALR FOR LIVE CHAT
builder.Services.AddSignalR();

// ✅ DATABASE
builder.Services.AddDbContext<PmsDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("PmsDbConnectionString")));

// ✅ IDENTITY
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

// ✅ REPOSITORIES
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IDeveloperRepository, DeveloperRepository>();
builder.Services.AddScoped<IImageRepo, CloudinaryImageRepo>();

// ✅ CHAT SERVICES (LIVE CHATTING)
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();

// ✅ AI ASSISTANT SERVICES (WITH FALLBACK)
builder.Services.AddHttpClient();
builder.Services.AddScoped<IDeepSeekService, DeepSeekService>();

var app = builder.Build();

await SeedService.SeedDatabase(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ✅ SIGNALR HUB MAPPING
app.MapHub<ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();