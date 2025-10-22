using Google.Cloud.Firestore;
using VeriWork_Admin.Application.Services;
using VeriWork_Admin.Core.Interfaces;
using VeriWork_Admin.Infrastructure.Config;
using VeriWork_Admin.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Firestore setup
string projectId = "veriwork-database";
string credentialPath = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure/Config/service-account.json");
string bucketName = "veriwork-database.firebasestorage.app";

FirestoreDb db = FirebaseInitializer.Initialize(projectId, credentialPath);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddSession(); // enable session support

// Dependency Injection
builder.Services.AddSingleton(db);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddSingleton(provider =>
    new FirebaseStorageService(projectId, bucketName, credentialPath));
builder.Services.AddScoped<AuditLogService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // use session middleware
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();