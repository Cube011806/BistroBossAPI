using BistroBossAPI.Data;
using BistroBossAPI.Models;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<Uzytkownik>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            // Pobieranie wartoœci z appsettings.json
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "ToMusiBycDlugiSekretnyKluczMin32Znaki!"))
        };
    });

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<BasketService>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddControllersWithViews()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

builder.Services.AddHttpClient();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

async Task SeedGuestUserAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Uzytkownik>>();

    // sprawdzamy czy u¿ytkownik GUEST ju¿ istnieje
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

app.Run();