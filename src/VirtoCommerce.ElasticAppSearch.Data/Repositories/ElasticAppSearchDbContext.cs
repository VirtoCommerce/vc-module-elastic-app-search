using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;

namespace VirtoCommerce.ElasticAppSearch.Data.Repositories
{
    public class ElasticAppSearchDbContext : DbContextWithTriggers
    {
        public ElasticAppSearchDbContext(DbContextOptions<ElasticAppSearchDbContext> options)
          : base(options)
        {
        }

        protected ElasticAppSearchDbContext(DbContextOptions options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //        modelBuilder.Entity<ElasticAppSearchEntity>().ToTable("MyModule").HasKey(x => x.Id);
            //        modelBuilder.Entity<ElasticAppSearchEntity>().Property(x => x.Id).HasMaxLength(128);
            //        base.OnModelCreating(modelBuilder);
        }
    }
}

