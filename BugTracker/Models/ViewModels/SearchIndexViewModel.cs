using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace BugTracker.Models.ViewModels
{
    public class SearchIndexViewModel
    {
        public List<Ticket> AdminPmTickets { get; set; } = new();
        public List<Ticket> DeveloperTickets { get; set; } = new();
        public List<Ticket> SubmitterTickets { get; set; } = new();
    }
}
