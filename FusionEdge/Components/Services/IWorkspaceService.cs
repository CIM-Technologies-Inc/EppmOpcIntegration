using FusionEdge.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionEdge.Components.Services
{
    internal interface IWorkspaceService
    {
        Task<List<Workspace>> GetWorkspaceAsync(string apiSoruce);
        Task<List<Project>> GetProjectsAsync(string workspace, string apiSource);

    }
}
