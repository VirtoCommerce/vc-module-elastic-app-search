using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VirtoCommerce.ElasticAppSearch.Data.Repositories
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ElasticAppSearchDbContext>
    {
        public ElasticAppSearchDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ElasticAppSearchDbContext>();

            builder.UseSqlServer("Data Source=(local);Initial Catalog=VirtoCommerce3;Persist Security Info=True;User ID=virto;Password=virto;MultipleActiveResultSets=True;Connect Timeout=30");

            return new ElasticAppSearchDbContext(builder.Options);
        }
    }
}
