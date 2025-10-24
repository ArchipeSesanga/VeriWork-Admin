using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using Microsoft.AspNetCore.Authentication.Cookies;
using VeriWork_Admin.Application.Services;
using VeriWork_Admin.Core.Interfaces;
using VeriWork_Admin.Infrastructure.Config;
using VeriWork_Admin.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// === FIREBASE CONFIG ===
string projectId = "veriwork-database";
string credentialPath = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure/Config/service-account.json");
string bucketName = "veriwork-database.firebasestorage.app";

// Initialize Firestore
FirestoreDb db = FirebaseInitializer.Initialize(projectId, credentialPath);

// ✅ Initialize Firebase Admin SDK (required for Firebase Authentication)
if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile(credentialPath)
    });
}

// === SERVICES & DEPENDENCIES ===
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton(db);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddSingleton(provider =>
    new FirebaseStorageService(projectId, bucketName, credentialPath));
builder.Services.AddScoped<FirebaseAuthService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
    });



// === MIDDLEWARE ===
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();