using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models.ViewModels
{
    public class DashboardViewModel
    {
        public Company Company { get; set; }
        public List<Project> Projects { get; set; }
        public List<Ticket> Tickets { get; set; }
        public List<BTUser> Users { get; set; }
        public List<BTUser> Developers { get; set; }
        public List<BTUser> Submitters { get; set; }
        public List<BTUser> ProjectManagers { get; set; }
        public BTUser User { get; set; }

    }
}
