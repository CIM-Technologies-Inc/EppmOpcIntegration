using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionEdge.Data.Models
{
    internal class EmailNotification
    {
        public int Id { get; set; }

        public string EmailTemplate { get; set; }

        public int EmailId { get; set; }
        
        public string ProjectId { get; set; }
    }
}
