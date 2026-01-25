using BistroBossAPI;
using BistroBossAPI.Data;
using BistroBossAPI.Models;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------
// DATABASE
// -------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// -------------------------------
// IDENTITY
// -------------------------------
builder.Services.AddDefaultIdentity<Uzytkownik>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// -------------------------------
// AUTHENTICATION (JWT + Identity)
// -------------------------------
builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]
                    ?? "ToMusiBycDlugiSekretnyKluczMin32Znaki!"
                ))
        };
    });

// -------------------------------
// SERVICES (DI)
// -------------------------------
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<BasketService>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ManageService>();

builder.Services.AddHttpClient(); 

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IEmailService, EmailService>();

// -------------------------------
// MVC + API
// -------------------------------
builder.Services.AddControllersWithViews()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

var app = builder.Build();

// -------------------------------
// MIDDLEWARE PIPELINE
// -------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles(); 

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// -------------------------------
// ROUTING
// -------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// -------------------------------
// SEED GUEST USER
// -------------------------------
async Task SeedGuestUserAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Uzytkownik>>();

    var existing = await userManager.FindByIdAsync("GUEST");
    if (existing != null)
        return;

    var guest = new Uzytkownik
    {
        Id = "GUEST",
        UserName = "guest",
        NormalizedUserName = "GUEST",
        Email = "",
        NormalizedEmail = "",
        EmailConfirmed = true
    };

    await userManager.CreateAsync(guest, "Guest123!");
}

await SeedGuestUserAsync(app);

// -------------------------------
app.Run();
