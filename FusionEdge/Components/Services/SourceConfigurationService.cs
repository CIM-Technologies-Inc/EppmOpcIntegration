using FusionEdge.Data;
using FusionEdge.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace FusionEdge.Components.Services
{
    public class SourceConfigurationService : ISourceConfigurationService
    {

        public async Task<SourceConfiguration?> GetSingleSourceConfiguration(int userId)
        {
            try
            {
                using var db = new AppDbContext();

                await db.Database.EnsureCreatedAsync();
           
                return await db.SourceConfigurations
                    .FirstOrDefaultAsync(s => s.UserId == userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
