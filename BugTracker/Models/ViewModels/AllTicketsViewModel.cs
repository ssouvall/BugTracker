using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace BugTracker.Models.ViewModels
{
    public class AllTicketsViewModel
    {
        public IPagedList<Ticket> AllTickets { get; set; }
        public IPagedList<Ticket> PMTickets { get; set; }

    }
}
