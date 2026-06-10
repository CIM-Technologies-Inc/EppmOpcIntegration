using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionEdge.Data.Models
{
    public class SourceConfiguration
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string SelectedSource { get; set; } = " ";

        public string Domain { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string PlainPass { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
