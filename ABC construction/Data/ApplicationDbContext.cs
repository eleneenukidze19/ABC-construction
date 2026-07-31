using ABC_construction.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ABC_construction.Data;

/// <summary>
/// EF Core context for the site. Inherits <see cref="IdentityDbContext{TUser}"/>
/// so the admin panel's Identity tables live in the same PostgreSQL database
/// and participate in the same migrations.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<CompanyInformation> CompanyInformation => Set<CompanyInformation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Must run first: creates the Identity schema this method then extends.
        base.OnModelCreating(builder);

        builder.Entity<Project>(entity =>
        {
            entity.HasIndex(p => p.Category);

            // Public listings filter on IsActive and sort by CompletionDate, so
            // index the pair rather than IsActive alone.
            entity.HasIndex(p => new { p.IsActive, p.CompletionDate });

            entity.HasMany(p => p.Images)
                  .WithOne(i => i.Project!)
                  .HasForeignKey(i => i.ProjectId)
                  // Gallery images have no meaning without their project.
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProjectImage>(entity =>
        {
            entity.HasIndex(i => new { i.ProjectId, i.SortOrder });
        });

        builder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => new { e.IsActive, e.SortOrder });
        });

        builder.Entity<CompanyInformation>(entity =>
        {
            // Seeded so /contact and the admin settings page always have a row
            // to read and edit. README section 9 fixes the initial values.
            entity.HasData(new CompanyInformation
            {
                Id = Models.CompanyInformation.SingletonId,
                Email = Models.CompanyInformation.Unwritten,
                PhoneNumber = Models.CompanyInformation.Unwritten,
                Address = Models.CompanyInformation.Unwritten,
                Description = null
            });
        });
    }
}
