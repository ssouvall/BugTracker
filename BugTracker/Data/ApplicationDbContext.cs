using BugTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BugTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<BTUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<BugTracker.Models.Company> Company { get; set; }
        public DbSet<BugTracker.Models.Invite> Invite { get; set; }
        public DbSet<BugTracker.Models.Notification> Notification { get; set; }
        public DbSet<BugTracker.Models.Project> Project { get; set; }
        public DbSet<BugTracker.Models.ProjectPriority> ProjectPriority { get; set; }
        public DbSet<BugTracker.Models.Ticket> Ticket { get; set; }
        public DbSet<BugTracker.Models.TicketAttachment> TicketAttachment { get; set; }
        public DbSet<BugTracker.Models.TicketComment> TicketComment { get; set; }
        public DbSet<BugTracker.Models.TicketHistory> TicketHistory { get; set; }
        public DbSet<BugTracker.Models.TicketPriority> TicketPriority { get; set; }
        public DbSet<BugTracker.Models.TicketStatus> TicketStatus { get; set; }
        public DbSet<BugTracker.Models.TicketType> TicketType { get; set; }
    }
}
