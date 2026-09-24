using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using midasMVC.Data;
using midasMVC.Models;
using midasMVC.Services;
using MySqlConnector;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<MovementRepository>();
builder.Services.AddScoped<MovementCategoryRepository>();
builder.Services.AddScoped<CuentasRepository>();
builder.Services.AddScoped<MetasRepository>();
builder.Services.AddScoped<SubscriptionRepository>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<BudgetRepository>();

var jwtKey = builder.Configuration["Jwt:Key"];

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultSignInScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.LogoutPath = "/Cuenta/Logout";
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;

        options.Cookie.Name = "midas_db.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey!)
            ),

            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();