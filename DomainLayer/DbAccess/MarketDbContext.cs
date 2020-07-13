using DomainLayer.Orders;
using DomainLayer.Stores;
using DomainLayer.Stores.Certifications;
using DomainLayer.Stores.Discounts;
using DomainLayer.Stores.Inventory;
using DomainLayer.Stores.PurchasePolicies;
using DomainLayer.Users;
using System.Data.Common;
using System.Data.Entity;

namespace DomainLayer.DbAccess
{
    public class MarketDbContext : DbContext
    {
        //public MarketDbContext() : base("Server = localhost\\SQLEXPRESS01; Database=WSEP202;Trusted_Connection=True;")
        //public MarketDbContext() : base("Server =DESKTOP-IL7RHOK\\SQLEXPRESS; Database=WSEP202;Trusted_Connection=True;")
        public MarketDbContext() : base("Server=127.0.0.1,1433;Database=WSEP202;User ID=SA;Password=Aa123456@;Connection Timeout=30;")
        {
            Database.CommandTimeout = 120;
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MarketDbContext, Migrations.Configuration>());
            //Database.SetInitializer(new DropCreateDatabaseAlways());
        }

        //use remote DB - need to open port in firewall first.
        //public MarketDbContext() : base("Server=tcp:wsep202.database.windows.net,1433;Initial Catalog=wsep202;Persist Security Info=False;User ID=wsep;Password=wsEbay1234!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;")
        //{
        //    Database.CommandTimeout = 120;
        //    Database.SetInitializer(new MigrateDatabaseToLatestVersion<MarketDbContext, Migrations.Configuration>());
        //}
        //For tests use in memory effort db
        public MarketDbContext(DbConnection connection) : base(connection, false)//base("Server=tcp:wsep-server.database.windows.net,1433;Initial Catalog=wsep202;Persist Security Info=False;User ID=wsep;Password=wsEbay1234!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;")
        {
            Database.CommandTimeout = 120;
            Database.SetInitializer(new DropCreateDatabaseAlways());
        }

        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Keyword> Keywords { get; set; }
        public virtual DbSet<Store> Stores { get; set; }
        public virtual DbSet<Certification> Certifications { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<StoreOrder> StoreOrders { get; set; }
        public virtual DbSet<ADiscount> Discounts { get; set; }
        public virtual DbSet<APurchasePolicy> Policies { get; set; }
        public virtual DbSet<RegisteredUser> Users { get; set; }
        public virtual DbSet<UserMessage> Messages { get; set; }
        public virtual DbSet<ShoppingCart> Carts { get; set; }
        public virtual DbSet<DailyStatistics> Stats { get; set; }

        public void Init() => Database.Initialize(true);

        private class DropCreateDatabaseAlways : DropCreateDatabaseAlways<MarketDbContext>
        {
            protected override void Seed(MarketDbContext context)
            {
                //base.Seed(context);
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Certification>().HasKey(c => new { c.UserID, c.StoreID });
            modelBuilder.Entity<OrderItem>().HasKey(oi => new { oi.ItemId, oi.StoreOrderId });
            modelBuilder.Entity<StoreOwnerApointmentContract>().HasKey(c => new { c.GranteeID, c.StoreID });
            modelBuilder.Entity<Approval>().HasKey(a => new { a.UserID, a.GranteeID, a.StoreID });
            modelBuilder.Entity<StoreOwnerApointmentContract>().HasRequired(a => a.Grantee)
                                                                .WithMany(u => u.PendingContracts)
                                                                .WillCascadeOnDelete(false);
            modelBuilder.Entity<StoreOwnerApointmentContract>().HasRequired(a => a.Grantor)
                                                                .WithMany(u => u.GrantorContracts)
                                                                .WillCascadeOnDelete(false);
            // enforce optional refernce to RegisteredUser
            //modelBuilder.Entity<Order>().HasOptional(o => o.User);
            modelBuilder.Entity<StoreCart>().HasKey(sc => new { sc.UserID, sc.StoreID });
            modelBuilder.Entity<ItemAmount>().HasKey(ia => new { ia.UserID, ia.StoreID, ia.ItemID });
        }
    }
}
