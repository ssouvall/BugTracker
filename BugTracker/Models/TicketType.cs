using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class TicketType
    {
        
        // Primary Key
        public int Id { get; set; }

        [DisplayName("Ticket Type")]
        public string Name { get; set; }



    }
}
