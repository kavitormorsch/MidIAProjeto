using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MidIAProjeto.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    
    public DbSet<MediaList> MediaList { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MediaList>()
            .ComplexProperty(ml => ml.GeneratedList, d => d.ToJson());
        base.OnModelCreating(modelBuilder);
    }
}

