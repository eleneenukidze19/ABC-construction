using ABC_construction.Configuration;
using ABC_construction.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ABC_construction.Data;

/// <summary>
/// Applies pending migrations and ensures the admin role and initial account
/// exist (README section 5).
/// </summary>
public static class DatabaseSeeder
{
    public static async Task MigrateAndSeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var logger = provider.GetRequiredService<ILogger<Program>>();
        var context = provider.GetRequiredService<ApplicationDbContext>();

        await CreateSchemaAsync(context, logger, cancellationToken);

        await SeedRolesAsync(provider, logger);
        await SeedAdminUserAsync(provider, logger);
    }

    /// <summary>
    /// Brings the database schema up to date.
    /// <para>
    /// The committed migration is generated for Npgsql and its SQL will not run
    /// on SQLite, so the dev fallback builds the schema directly from the model
    /// instead. EnsureCreated still applies the HasData seed, so the
    /// CompanyInformation row exists either way.
    /// </para>
    /// <para>
    /// The trade-off: a SQLite database has no migration history and will not
    /// pick up later schema changes incrementally — delete the .db file to
    /// rebuild it. That is acceptable for local development only; PostgreSQL
    /// remains the migrated, production path.
    /// </para>
    /// </summary>
    private static async Task CreateSchemaAsync(
        ApplicationDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var isSqlite = context.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;

        if (isSqlite)
        {
            var created = await context.Database.EnsureCreatedAsync(cancellationToken);

            logger.LogWarning(
                created
                    ? "SQLite dev database created from the model (no migration history). Use PostgreSQL for production."
                    : "Using the existing SQLite dev database. Delete the .db file to rebuild it after model changes.");

            return;
        }

        await context.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Database migrations applied.");
    }

    /// <summary>
    /// Seeds every role in <see cref="ApplicationRoles.All"/>, so introducing a
    /// role later needs only a new constant — no change here.
    /// </summary>
    private static async Task SeedRolesAsync(IServiceProvider provider, ILogger logger)
    {
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in ApplicationRoles.All)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(role));

            if (result.Succeeded)
            {
                logger.LogInformation("Created role {Role}.", role);
            }
            else
            {
                logger.LogError(
                    "Could not create role {Role}: {Errors}",
                    role,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private static async Task SeedAdminUserAsync(IServiceProvider provider, ILogger logger)
    {
        var options = provider.GetRequiredService<IOptions<AdminSeedOptions>>().Value;

        // The password is never committed (see appsettings.json), so on a fresh
        // checkout this is the expected path until an operator supplies one.
        // Failing quietly would leave an unreachable panel with no explanation.
        if (string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
        {
            logger.LogWarning(
                "No initial admin account was created: AdminSeed:Email/Password is not configured. " +
                "Set the password with `dotnet user-secrets set \"AdminSeed:Password\" \"<password>\"` " +
                "in development, or the AdminSeed__Password environment variable elsewhere, then restart.");
            return;
        }

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        if (await userManager.FindByEmailAsync(options.Email) is not null)
        {
            return;
        }

        // Multiple admins are supported (README section 5); this only bootstraps
        // the first one so the panel is reachable on a fresh database.
        var admin = new ApplicationUser
        {
            UserName = options.Email,
            Email = options.Email,
            EmailConfirmed = true,
            DisplayName = "Administrator",
            CreatedDate = DateTime.UtcNow
        };

        var created = await userManager.CreateAsync(admin, options.Password);

        if (!created.Succeeded)
        {
            logger.LogError(
                "Could not create the seed admin account: {Errors}",
                string.Join("; ", created.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(admin, ApplicationRoles.Admin);

        logger.LogInformation(
            "Seeded admin account {Email}. Change this password before deploying.", options.Email);
    }
}
