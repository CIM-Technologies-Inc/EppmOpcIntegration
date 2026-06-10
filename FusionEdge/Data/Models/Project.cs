using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FusionEdge.Data.Models
{
    public class Project
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string ObjectId { get; set; } = "";
        public string ParentEPSObjectId { get; set; } = "";
    }
}
