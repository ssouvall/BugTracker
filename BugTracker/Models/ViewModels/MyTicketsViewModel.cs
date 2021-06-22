using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace BugTracker.Models.ViewModels
{
    public class MyTicketsViewModel
    {
        public IPagedList<Ticket> DevTickets { get; set; }
        public IPagedList<Ticket> SubmittedTickets { get; set; }

    }
}
