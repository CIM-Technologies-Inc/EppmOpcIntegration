using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionEdge.Data.Models
{
    internal class ScheduleSetting
    {
        public int Id { get; set; }
        public string Workspace { get; set; } = "";
        public string Project { get; set; } = "";

        public string Projectname { get; set; } = "";
        public string ScheduleType { get; set; } = "";
        public string Days { get; set; } = "";
        public TimeSpan Time { get; set; }
        
        public DateTime DateTimePublish { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsExecuted { get; set; } = false;

        public DateTime? LastExecuted { get; set; }

        public int UserId { get; set; }
    }
}
