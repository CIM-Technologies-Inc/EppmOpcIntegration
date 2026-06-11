using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionEdge.Data.Models
{
    public class EmailReceiver
    {
        public int Id { get; set; }

        public string Email { get; set; } = "";

        public int ProjectId { get; set; }
    }
}
