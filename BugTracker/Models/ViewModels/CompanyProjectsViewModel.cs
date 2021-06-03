using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models.ViewModels
{
    public class CompanyProjectsViewModel
    {
        public List<Project> Projects { get; set; } = new();
        public List<Ticket> Tickets { get; set; } = new();
        public Company Company { get; set; }
    }
}
