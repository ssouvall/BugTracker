using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace BugTracker.Models.ViewModels
{
    public class SearchIndexViewModel
    {
        public IPagedList<Ticket> AdminPmTickets { get; set; }
        public IPagedList<Ticket> DeveloperTickets { get; set; }
        public IPagedList<Ticket> SubmitterTickets { get; set; }
    }
}
