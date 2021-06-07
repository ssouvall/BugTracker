using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models.ViewModels
{
    public class MyTicketsViewModel
    {
        public IEnumerable<Ticket> DevTickets { get; set; }
        public IEnumerable<Ticket> SubmittedTickets { get; set; }

    }
}
