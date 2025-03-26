using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebADS.Data;
using WebADS.Models;
using WebADS.Models.Token;
using WebADS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache(); // Используем in-memory хранилище
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromHours(1); // Сессия на 1 час
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()    // Разрешаем любой источник (для мобильных устройств)
            .AllowAnyMethod()    // Разрешаем любые HTTP методы (GET, POST, PUT и т.д.)
            .AllowAnyHeader()    // Разрешаем любые заголовки
            .AllowCredentials(); // Разрешаем отправку cookies
    });
});



builder.Services.AddDbContext<ADSContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AuthContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("IdentityConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = false; // ������ ����� � ��� UserName
}).AddEntityFrameworkStores<AuthContext>()
    .AddDefaultTokenProviders();

builder.Services.AddHttpClient<MyStorageRequester>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5); // Устанавливаем глобальный таймаут 5 минут
});
builder.Services.AddScoped<MoySkladTokenHandler>();
builder.Services.AddScoped<MyStorageAPIModel>();
builder.Services.AddScoped<MasterToken>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ADSHandler>();
builder.Services.AddScoped<ADSReports>();
builder.Services.AddScoped<ArticleParametersHandler>();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/AdsIdentity/Account/Login"; // ����� ���� �������� �����
    options.AccessDeniedPath = "/AdsIdentity/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ��������� HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ADSContext>();
    context.Database.Migrate();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapFallbackToPage("/ADSIndex");

app.Run();
