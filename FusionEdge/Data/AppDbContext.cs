using FusionEdge.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionEdge.Data
{
    internal class AppDbContext : DbContext
    {
        public DbSet<ScheduleSetting> ScheduleSettings => Set<ScheduleSetting>();
        public DbSet<User> User => Set<User>();

        public DbSet<SourceConfiguration> SourceConfigurations => Set<SourceConfiguration>();

        public DbSet<EmailReceiver> EmailReceivers => Set<EmailReceiver>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // Use MAUI AppDataDirectory for cross-platform storage
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "fusion-edge.db");

            options.UseSqlite($"Data Source={dbPath}");
            Console.WriteLine("Database Path: " + dbPath);
        }
    }
}
