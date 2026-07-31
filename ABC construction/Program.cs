using System.Threading.RateLimiting;
using ABC_construction.Configuration;
using ABC_construction.Data;
using ABC_construction.Interfaces;
using ABC_construction.Middleware;
using ABC_construction.Models;
using ABC_construction.Repositories;
using ABC_construction.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Configuration binding
// ---------------------------------------------------------------------------
builder.Services.Configure<CachingOptions>(
    builder.Configuration.GetSection(CachingOptions.SectionName));
builder.Services.Configure<FileUploadOptions>(
    builder.Configuration.GetSection(FileUploadOptions.SectionName));
builder.Services.Configure<RateLimitingOptions>(
    builder.Configuration.GetSection(RateLimitingOptions.SectionName));
builder.Services.Configure<AdminSeedOptions>(
    builder.Configuration.GetSection(AdminSeedOptions.SectionName));

var rateLimitingOptions = builder.Configuration
    .GetSection(RateLimitingOptions.SectionName)
    .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

// ---------------------------------------------------------------------------
// Database (README section 8)
//
// PostgreSQL is the production target. SQLite exists only so the site can be
// run locally without installing a database server — see DatabaseSeeder for
// how the two differ at startup.
// ---------------------------------------------------------------------------
var useSqlite = string.Equals(
    builder.Configuration["Database:Provider"], "Sqlite", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useSqlite)
    {
        var sqliteConnection = builder.Configuration.GetConnectionString("SqliteConnection")
            ?? "Data Source=abc_construction.db";

        options.UseSqlite(sqliteConnection);
    }
    else
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not configured.");

        options.UseNpgsql(connectionString);
    }

    if (builder.Environment.IsDevelopment())
    {
        // Helpful locally; deliberately off elsewhere because both can leak
        // parameter values containing personal data into logs.
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// ---------------------------------------------------------------------------
// Identity (README section 5)
// ---------------------------------------------------------------------------
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.User.RequireUniqueEmail = true;

        // Slows down password guessing against the admin panel.
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/admin/login";
    options.LogoutPath = "/admin/logout";
    options.AccessDeniedPath = "/admin/denied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;

    // Always outside Development so the auth cookie never crosses plain HTTP.
    // In Development that would mean the cookie is silently dropped when the
    // site is started on the http-only launch profile, making sign-in appear
    // to succeed and then do nothing.
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    // Strict is the CSRF defence for the cookie-authenticated /api writes:
    // the browser will not attach this cookie to cross-site requests.
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// ---------------------------------------------------------------------------
// Application layers: Repositories -> Services -> Controllers (README section 7)
// ---------------------------------------------------------------------------
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ICompanyInformationRepository, CompanyInformationRepository>();

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
// Wraps Identity's UserManager so admin-account rules stay out of controllers.
builder.Services.AddScoped<IUserAdminService, UserAdminService>();

// Caching (README section 17). Singleton: the cache outlives any one request.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// ---------------------------------------------------------------------------
// MVC + API
// ---------------------------------------------------------------------------
builder.Services.AddControllersWithViews(options =>
{
    // Anti-forgery on every non-GET MVC form post (README section 19).
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddOpenApi();

// ---------------------------------------------------------------------------
// Rate limiting (README section 18)
// ---------------------------------------------------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        // Authenticated admins get the higher allowance, partitioned per user
        // so one admin cannot exhaust another's budget. Anonymous traffic is
        // partitioned per IP.
        var isAuthenticated = context.User.Identity?.IsAuthenticated == true;

        var partitionKey = isAuthenticated
            ? $"user:{context.User.Identity!.Name}"
            : $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

        var permitLimit = isAuthenticated
            ? rateLimitingOptions.AuthenticatedPermitPerMinute
            : rateLimitingOptions.AnonymousPermitPerMinute;

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = rateLimitingOptions.QueueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiter");

        logger.LogWarning(
            "Rate limit hit for {Path} from {Ip}.",
            context.HttpContext.Request.Path,
            context.HttpContext.Connection.RemoteIpAddress);

        // Tell well-behaved clients when to retry.
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();
        }

        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again shortly.", cancellationToken);
    };
});

// Static assets are fingerprinted and cached hard (README section 23).
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);

var app = builder.Build();

// ---------------------------------------------------------------------------
// HTTP pipeline — order matters
// ---------------------------------------------------------------------------

// First, so it wraps everything downstream.
app.UseGlobalExceptionHandling();

// Re-runs the pipeline against the error action for bare status codes (mainly
// 404) so visitors get the styled page instead of an empty browser default.
//
// Applied to HTML routes only: a REST client asking for /api/* wants the status
// code and a ProblemDetails body, not a rendered error page. Re-executing there
// also rewrites the response of legitimate 401/403/404 API replies.
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase),
    branch => branch.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}"));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    // HSTS only outside Development to avoid pinning localhost to HTTPS.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.CacheControl = "public,max-age=604800";
    }
});

app.UseRouting();

// Rate limiting after routing so endpoint metadata is available, but before
// auth so throttled traffic never reaches the database.
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Attribute-routed API controllers (/api/*).
app.MapControllers();

await DatabaseSeeder.MigrateAndSeedAsync(app.Services);

app.Run();
