﻿using Microsoft.EntityFrameworkCore;

namespace Common
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Common.SessionSettings> Sessions { get; set; }
        public DbSet<Common.Collector> Collectors { get; set; }

        #region Required
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Common.Collector>().HasKey(c => c.Phone);
            modelBuilder.Entity<Common.SessionSettings>().HasKey(c => c.SessionStorageHost);
        }
        #endregion

        public ApplicationContext()
        {
            //try
            //{
            //   //var databaseCreator = (Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator);
            //   // databaseCreator.CreateTables();
            //}
            //catch (Exception ex)
            //{
            //    //A SqlException will be thrown if tables already exist. So simply ignore it.
            //}
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Options.ConnectionString1);
        }
    }
}
