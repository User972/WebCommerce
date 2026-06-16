using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using SirJoshua.Web.Data;
using SirJoshua.Web.Middleware;
using SirJoshua.Web.Options;
using SirJoshua.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Configuration / options ----
builder.Services
    .AddOptions<PayPalOptions>()
    .Bind(builder.Configuration.GetSection(PayPalOptions.SectionName))
    .ValidateDataAnnotations();

builder.Services
    .AddOptions<NotificationOptions>()
    .Bind(builder.Configuration.GetSection(NotificationOptions.SectionName));

// ---- Database (PostgreSQL via Npgsql) ----
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not configured.");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));

// ---- HTTP client for PayPal ----
builder.Services.AddHttpClient<IPayPalClient, PayPalClient>();
builder.Services.AddScoped<IOrderService, OrderService>();

// ---- Lead capture / enquiry funnel ----
builder.Services.AddScoped<IEnquiryService, EnquiryService>();
builder.Services.AddSingleton<INotificationService, LoggingNotificationService>();

// ---- MVC + anti-forgery ----
builder.Services.AddControllersWithViews(options =>
{
    // Require a valid anti-forgery token on every unsafe (POST/PUT/DELETE) request.
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

// The browser reads the token from a cookie and echoes it in this header on fetch() calls.
builder.Services.AddAntiforgery(o => o.HeaderName = "X-CSRF-TOKEN");

builder.Services.AddHsts(o =>
{
    o.Preload = true;
    o.IncludeSubDomains = true;
    o.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

// Apply pending migrations and ensure the catalog is seeded at startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Respect X-Forwarded-* from a TLS-terminating reverse proxy.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSecurityHeaders();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
