using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataFair
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Common.UserInfo> UsersInfo { get; set; }
        public DbSet<Common.SessionSettings> Sessions { get; set; }
        public DbSet<Common.Collector> Collectors { get; set; }

        #region Required
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Common.UserInfo>().HasKey(c=>c.Phone);
            modelBuilder.Entity<Common.Collector>().HasKey(c => c.ApiId);
            modelBuilder.Entity<Common.SessionSettings>().HasKey(c => c.SessionStorageHost);
        }
        #endregion

        public ApplicationContext()
        {
            try
            {
                var databaseCreator = (Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator);
                databaseCreator.CreateTables();
            }
            catch (Exception ex)
            {
                var w = 0;
                //A SqlException will be thrown if tables already exist. So simply ignore it.
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql();
        }
    }
}
