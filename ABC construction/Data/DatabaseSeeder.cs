using System.Reflection;
using ABC_construction.Configuration;
using ABC_construction.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;

namespace ABC_construction.Data;

/// <summary>
/// Applies pending migrations, ensures the admin role and initial account
/// exist (README section 5), and loads <see cref="InitialContent"/> into a new
/// database.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task MigrateAndSeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var logger = provider.GetRequiredService<ILogger<Program>>();
        var context = provider.GetRequiredService<ApplicationDbContext>();

        var isNewContentStore = await CreateSchemaAsync(context, logger, cancellationToken);

        await SeedRolesAsync(provider, logger);
        await SeedAdminUserAsync(provider, logger);

        if (isNewContentStore)
        {
            await SeedInitialContentAsync(context, logger, cancellationToken);
        }
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
    /// <returns>
    /// True the one time a database should receive <see cref="InitialContent"/>:
    /// when the SQLite file was just created, or when this run applied the
    /// PostgreSQL migration that introduced that content. Tying it to a
    /// one-off event rather than "the table is empty" matters because admin
    /// deletes are hard deletes — an empty-table check would restore every
    /// deleted project on the next restart.
    /// </returns>
    private static async Task<bool> CreateSchemaAsync(
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

            return created;
        }

        var pending = await context.Database.GetPendingMigrationsAsync(cancellationToken);

        await context.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Database migrations applied.");

        return pending.Contains(InitialContentMigrationId);
    }

    private static readonly string InitialContentMigrationId =
        typeof(Migrations.MakeProjectDateAndDurationOptional).GetCustomAttribute<MigrationAttribute>()!.Id;

    /// <summary>
    /// Loads the checklist projects and team. Each table is only filled if it
    /// is still empty, so records an admin entered before upgrading are never
    /// mixed with the defaults.
    /// </summary>
    private static async Task SeedInitialContentAsync(
        ApplicationDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        if (!await context.Projects.AnyAsync(cancellationToken))
        {
            // Undated projects list highest Id first, so inserting in reverse
            // shows them on the site in checklist order.
            context.Projects.AddRange(InitialContent.CreateProjects(now).Reverse());
        }

        if (!await context.Employees.AnyAsync(cancellationToken))
        {
            context.Employees.AddRange(InitialContent.CreateEmployees(now));
        }

        var added = await context.SaveChangesAsync(cancellationToken);

        if (added > 0)
        {
            logger.LogInformation("Loaded {Count} initial projects and team members.", added);
        }
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
