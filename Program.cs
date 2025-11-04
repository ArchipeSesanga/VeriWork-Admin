using System.Net.Http.Headers;
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

// === DEPENDENCIES ===
builder.Services.AddSingleton(db);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddSingleton(provider =>
    new FirebaseStorageService(projectId, bucketName, credentialPath));
builder.Services.AddScoped<FirebaseAuthService>();

// ✅ Azure Face Client setup
var faceConfig = builder.Configuration.GetSection("AzureFace");
builder.Services.AddHttpClient("AzureFace", client =>
{
    client.BaseAddress = new Uri(faceConfig["Endpoint"]);
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", faceConfig["SubscriptionKey"]);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddSingleton<FaceService>();

// === AUTHENTICATION ===

// ✅ Unified setup for both cookie (admin web) and JWT (API)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            context.Token = token;
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                var firebaseToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
                context.Principal.AddIdentity(new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("uid", firebaseToken.Uid)
                }));
            }
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };
});

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// === MIDDLEWARE ===
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();  // ✅ Must come before Authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
