using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class Invite
    {
        public int Id { get; set; }
        public DateTimeOffset InviteDate { get; set; }
        public Guid CompanyToken { get; set; }
        public int CompanyId { get; set; }
        public int ProjectId { get; set; }
        public string InvitorId { get; set; }
        public string InviteeId { get; set; }
        public string InviteeEmail { get; set; }
        public string InviteeFirstName { get; set; }
        public string InviteeLastName { get; set; }
        public bool IsValid { get; set; }
        public virtual Company Company { get; set; }
        public virtual BTUser Invitor { get; set; }
        public virtual BTUser Invitee { get; set; }
        public virtual Project Project { get; set; }

    }
}
