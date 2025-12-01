using FastPMS.Data;
using FastPMS.Hubs;
using FastPMS.ImageRepository;
using FastPMS.Models.Domain;
using FastPMS.Repositories;
using FastPMS.Repositories.Interfaces;
using FastPMS.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ✅ SIGNALR FOR LIVE CHAT & NOTIFICATIONS
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400; // 100KB
});

// ✅ DATABASE
builder.Services.AddDbContext<PmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PmsDbConnectionString")));

// ✅ IDENTITY
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<PmsDbContext>()
.AddDefaultTokenProviders();

// ✅ REPOSITORIES
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IDeveloperRepository, DeveloperRepository>();
builder.Services.AddScoped<IImageRepo, CloudinaryImageRepo>();

// ✅ SERVICES
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IDeepSeekService, DeepSeekService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ISubTaskService, SubTaskService>();

// ✅ REPOSITORIES
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<ISubTaskRepository, SubTaskRepository>();
// builder.Services.AddScoped<INotificationRepository, NotificationRepository>(); // যদি থাকে

// ✅ AI ASSISTANT SERVICES
builder.Services.AddHttpClient();

var app = builder.Build();

await SeedService.SeedDatabase(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ✅ SIGNALR HUB MAPPING - VERIFY THESE
app.MapHub<ChatHub>("/chatHub");
app.MapHub<NotificationHub>("/notificationHub"); // ✅ ADD THIS LINE

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();