using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Entities
{
    public class Tunnel
    {
        public int Id { get; set; }                     
        public int LocalPort { get; set; }
        public required string PublicUrl { get; set; }
        public required string DashboardUrl { get; set; }
        public string Protocol { get; set; } = "http";  
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }          
        public string Status { get; set; } = "Active";  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
