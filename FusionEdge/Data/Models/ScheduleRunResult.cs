using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionEdge.Data.Models
{
    public class ScheduleRunResult
    {
        public int ScheduleId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string Project { get; set; } = "";
        public DateTime Time { get; set; } = DateTime.Now;
    }
}
