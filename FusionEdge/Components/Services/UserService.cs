using FusionEdge.Data.Models;

namespace FusionEdge.Components.Services
{
    public class UserService
    {
        public User CurrentUser { get; set; } = new();
    }
}