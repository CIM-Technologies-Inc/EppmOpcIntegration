using FusionEdge.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionEdge.Components.Services
{
    
    interface IFileTransferService
    {
        Task<string> MoveFileAsync(string sourcepath, string destinationfolder);
        Task<string> ExportAndSave(string Workspace, long projectId, SourceConfiguration config, string projectName, int UserId);
    }
}
