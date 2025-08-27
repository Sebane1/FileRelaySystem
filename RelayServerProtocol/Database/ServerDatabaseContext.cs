using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RelayServerProtocol.Database
{
    public class ServerDatabasContext : DbContext
    {
        public DbSet<ServerData> ServerData { get; set; }
        public DbSet<PersistedSessionData> Sessions { get; set; }
        public DbSet<UnclaimedKey> UnclaimedKeys { get; set; }

        public ServerDatabasContext(DbContextOptions<ServerDatabasContext> options)
            : base(options)
        {
            Database.EnsureCreated(); // auto-create schema if not exists
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ensure single row for ServerData
            modelBuilder.Entity<ServerData>().HasData(new ServerData { Id = 1 });
        }

        public class UnclaimedKey
        {
            [Key]
            public string KeyHash { get; set; }
        }
    }
}
