using FusionEdge.Data.DTOs;
using FusionEdge.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionEdge.Components.Services
{
    internal interface IAuthenticationService
    {


        Task<User?> Login(string username, string password);

        Task<bool> RegisterAsync(CreateUserDto dto);
        Task<bool> AuthenticateCredential(string username, string password);

        Task<bool> SaveSourceConfiguration(SourceSettingDto dto);

        Task<List<string>> GetProjectEmails(string projectId);
    }
}
