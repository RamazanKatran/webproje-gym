using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebProjeGym.Data;
using WebProjeGym.Models;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        // Ödev gereği admin şifresi 'sau' olabilsin diye parola kurallarını basitleştiriyoruz
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 3;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// #region agent log
System.IO.File.AppendAllText(
    "c:\\Users\\ASUS\\Desktop\\Hafta2Web\\WebProjeGym\\.cursor\\debug.log",
    "{\"sessionId\":\"debug-session\",\"runId\":\"pre-fix\",\"hypothesisId\":\"H1\",\"location\":\"Program.cs:20\",\"message\":\"Configured identity with ApplicationUser\",\"data\":{\"userType\":\"ApplicationUser\"},\"timestamp\":"
    + DateTimeOffset.Now.ToUnixTimeMilliseconds() + "}\n");
System.IO.File.AppendAllText(
    "c:\\Users\\ASUS\\Desktop\\Hafta2Web\\WebProjeGym\\.cursor\\debug.log",
    "{\"sessionId\":\"debug-session\",\"runId\":\"post-fix\",\"hypothesisId\":\"V1\",\"location\":\"Program.cs:21\",\"message\":\"Post-fix verification startup\",\"data\":{},\"timestamp\":"
    + DateTimeOffset.Now.ToUnixTimeMilliseconds() + "}\n");
// #endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
