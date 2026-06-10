using FusionEdge.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionEdge.Components.Services
{
    public interface ISourceConfigurationService
    {
        Task<SourceConfiguration?> GetSingleSourceConfiguration(int userId);
    }
}
