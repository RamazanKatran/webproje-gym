using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebProjeGym.Data;
using WebProjeGym.Models;
using WebProjeGym.Services;


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

// Gemini Service kaydı
builder.Services.AddHttpClient<GeminiService>();

// #region agent log
try
{
    var logPath = "c:\\Users\\ASUS\\Desktop\\Hafta2Web\\WebProjeGym\\.cursor\\debug.log";
    var logDir = System.IO.Path.GetDirectoryName(logPath);
    if (!System.IO.Directory.Exists(logDir))
    {
        System.IO.Directory.CreateDirectory(logDir ?? "");
    }
    System.IO.File.AppendAllText(logPath,
        "{\"sessionId\":\"debug-session\",\"runId\":\"pre-fix\",\"hypothesisId\":\"H1\",\"location\":\"Program.cs:20\",\"message\":\"Configured identity with ApplicationUser\",\"data\":{\"userType\":\"ApplicationUser\"},\"timestamp\":"
        + DateTimeOffset.Now.ToUnixTimeMilliseconds() + "}\n");
    System.IO.File.AppendAllText(logPath,
        "{\"sessionId\":\"debug-session\",\"runId\":\"post-fix\",\"hypothesisId\":\"V1\",\"location\":\"Program.cs:21\",\"message\":\"Post-fix verification startup\",\"data\":{},\"timestamp\":"
        + DateTimeOffset.Now.ToUnixTimeMilliseconds() + "}\n");
}
catch { }
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
    
    // #region agent log
    try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "H3", location = "Program.cs:54", message = "Before SeedData", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
    // #endregion

    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    
    // #region agent log
    try 
    { 
        // Veritabanında ApplicationUserId kolonunun olup olmadığını kontrol et
        var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Trainers' AND COLUMN_NAME = 'ApplicationUserId'";
        var columnExists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        await connection.CloseAsync();
        
        await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "H4", location = "Program.cs:66", message = "Column existence check", data = new { columnExists = columnExists }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
        
        // Eğer kolon yoksa migration'ı zorla uygula
        if (!columnExists)
        {
            await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "H4", location = "Program.cs:70", message = "Column missing, applying migration manually", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
            
            // Migration'ı manuel olarak SQL ile uygula
            await connection.OpenAsync();
            var migrationCommand = connection.CreateCommand();
            migrationCommand.CommandText = @"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Trainers' AND COLUMN_NAME = 'ApplicationUserId')
                BEGIN
                    ALTER TABLE Trainers ADD ApplicationUserId NVARCHAR(450) NULL;
                    CREATE INDEX IX_Trainers_ApplicationUserId ON Trainers(ApplicationUserId);
                    ALTER TABLE Trainers ADD CONSTRAINT FK_Trainers_AspNetUsers_ApplicationUserId 
                        FOREIGN KEY (ApplicationUserId) REFERENCES AspNetUsers(Id) ON DELETE NO ACTION;
                END";
            await migrationCommand.ExecuteNonQueryAsync();
            await connection.CloseAsync();
            
            await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "H4", location = "Program.cs:84", message = "Migration applied manually", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
        }
        
        // MemberProfiles tablosuna FirstName ve LastName kolonlarını ekle (yoksa)
        try
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
            
            var firstNameCheckCommand = connection.CreateCommand();
            firstNameCheckCommand.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MemberProfiles' AND COLUMN_NAME = 'FirstName'";
            var firstNameExists = Convert.ToInt32(await firstNameCheckCommand.ExecuteScalarAsync()) > 0;
            
            if (!firstNameExists)
            {
                var addFirstNameCommand = connection.CreateCommand();
                addFirstNameCommand.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MemberProfiles' AND COLUMN_NAME = 'FirstName')
                    BEGIN
                        ALTER TABLE MemberProfiles ADD FirstName NVARCHAR(50) NULL;
                    END";
                await addFirstNameCommand.ExecuteNonQueryAsync();
            }
            
            var lastNameCheckCommand = connection.CreateCommand();
            lastNameCheckCommand.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MemberProfiles' AND COLUMN_NAME = 'LastName'";
            var lastNameExists = Convert.ToInt32(await lastNameCheckCommand.ExecuteScalarAsync()) > 0;
            
            if (!lastNameExists)
            {
                var addLastNameCommand = connection.CreateCommand();
                addLastNameCommand.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MemberProfiles' AND COLUMN_NAME = 'LastName')
                    BEGIN
                        ALTER TABLE MemberProfiles ADD LastName NVARCHAR(50) NULL;
                    END";
                await addLastNameCommand.ExecuteNonQueryAsync();
            }
        }
        catch (Exception memberProfileEx)
        {
            try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "H6", location = "Program.cs:102", message = "Error adding FirstName/LastName to MemberProfiles", data = new { exceptionType = memberProfileEx.GetType().Name, message = memberProfileEx.Message }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        }

        // AIRecommendations tablosunun varlığını kontrol et ve yoksa oluştur
        try
        {
            // Mevcut connection'ı kullan (zaten açık olabilir)
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
            
            var tableCheckCommand = connection.CreateCommand();
            tableCheckCommand.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AIRecommendations'";
            var tableExists = Convert.ToInt32(await tableCheckCommand.ExecuteScalarAsync()) > 0;
            
            if (!tableExists)
            {
                // Tablo yoksa migration'ı manuel olarak uygula
                var createTableCommand = connection.CreateCommand();
                createTableCommand.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AIRecommendations')
                    BEGIN
                        CREATE TABLE [AIRecommendations] (
                            [Id] int NOT NULL IDENTITY,
                            [ApplicationUserId] nvarchar(450) NOT NULL,
                            [RecommendationType] int NOT NULL,
                            [Content] nvarchar(max) NOT NULL,
                            [CreatedAt] datetime2 NOT NULL,
                            [HeightCm] int NOT NULL,
                            [WeightKg] real NOT NULL,
                            [Age] int NOT NULL,
                            [BodyType] nvarchar(50) NOT NULL,
                            [Goal] nvarchar(200) NOT NULL,
                            CONSTRAINT [PK_AIRecommendations] PRIMARY KEY ([Id]),
                            CONSTRAINT [FK_AIRecommendations_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
                        );
                        CREATE INDEX [IX_AIRecommendations_ApplicationUserId] ON [AIRecommendations] ([ApplicationUserId]);
                    END";
                await createTableCommand.ExecuteNonQueryAsync();
            }
        }
        catch (Exception aiTableEx)
        {
            // Tablo oluşturma hatası - logla ama devam et
            try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "H5", location = "Program.cs:102", message = "Error creating AIRecommendations table", data = new { exceptionType = aiTableEx.GetType().Name, message = aiTableEx.Message }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        }
        
        // Connection'ı kapat (eğer açıksa)
        if (connection.State == System.Data.ConnectionState.Open)
        {
            await connection.CloseAsync();
        }
        
        // Migration history'yi güncelle
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        var pendingList = pendingMigrations.ToList();
        await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "H4", location = "Program.cs:89", message = "Pending migrations check", data = new { pendingMigrations = pendingList, count = pendingList.Count }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
        
        if (pendingList.Count > 0)
        {
            await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "H4", location = "Program.cs:93", message = "Applying pending migrations", data = new { migrations = pendingList }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
            await dbContext.Database.MigrateAsync();
            await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "H4", location = "Program.cs:96", message = "Migrations applied successfully", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
        }
    } 
    catch (Exception ex)
    {
        try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "H4", location = "Program.cs:100", message = "Error checking/applying migrations", data = new { exceptionType = ex.GetType().Name, message = ex.Message, stackTrace = ex.StackTrace != null ? ex.StackTrace.Substring(0, Math.Min(500, ex.StackTrace.Length)) : null }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
    }
    // #endregion

    await SeedData.InitializeAsync(services);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
