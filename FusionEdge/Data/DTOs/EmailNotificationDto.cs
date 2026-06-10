using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionEdge.Data.DTOs
{
    public class EmailNotificationDto
    {
        public string EmailTemplate { get; set; }
        public int EmailId { get; set; }
        public string ProjectId { get; set; }
    }
}
