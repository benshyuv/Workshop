using DomainLayer.DbAccess;
using System.Data.Entity.Migrations;

namespace DomainLayer.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<MarketDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            CommandTimeout = 120;
        }

        protected override void Seed(MarketDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}
