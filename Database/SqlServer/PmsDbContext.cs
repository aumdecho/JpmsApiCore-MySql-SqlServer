using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JpmsApiCore_MySql.Database.SqlServer
{
    public class PmsDbContext : DbContext
    {
        public DbSet<TbGate> TbGates { get; set; }
        public DbSet<TbZone> TbZones { get; set; }
        public DbSet<TbBuilding> TbBuildings { get; set; }
        public DbSet<TbSystemInfo> TbSystemInfos { get; set; }
        public DbSet<TbCompanyInfo> TbCompanyInfos { get; set; }
        public DbSet<TbParkingCard> TbParkingCards { get; set; }
        public DbSet<TbParkingCardMap> TbParkingCardMaps { get; set; }
        public DbSet<TbCardType> TbCardTypes { get; set; }
        public DbSet<TbMember> TbMembers { get; set; }
        public DbSet<TbVehicle> TbVehicles { get; set; }
        public DbSet<TbCurrentParking> TbCurrentParkings { get; set; }
        public DbSet<TbParking> TbParkings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var connStr = config.GetValue<string>("ConnectionStrings:pmsDBContext", "");
            optionsBuilder.UseSqlServer(connStr);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TbParkingCardMap>().HasNoKey();
        }
    }
}
