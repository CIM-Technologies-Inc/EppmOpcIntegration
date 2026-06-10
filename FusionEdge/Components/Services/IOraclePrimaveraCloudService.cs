using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionEdge.Components.Services
{
    internal interface IOraclePrimaveraCloudService
    {
        Task<bool> AuthenticateOPCCredentials(string username, string password);
    }
}
