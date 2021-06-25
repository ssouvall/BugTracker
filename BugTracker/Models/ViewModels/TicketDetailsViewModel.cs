using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models.ViewModels
{
    public class TicketDetailsViewModel
    {
        public Ticket Ticket { get; set; }
        public BTUser ProjectManager { get; set; }
        public BTUser CurrentUser { get; set; }
    }
}
