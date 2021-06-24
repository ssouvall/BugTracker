using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugTracker.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTSearchService
    {
        //give ability to talk to the database
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;

        public BTSearchService(ApplicationDbContext context,
                            UserManager<BTUser> userManager,
                            IBTTicketService ticketService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
        }

        public IOrderedQueryable<Ticket> SearchContent(string searchString, int companyId)
        {
            var result = _context.Ticket.Where(t => t.Project.CompanyId == companyId);

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                result = result.Where(t => t.Title.ToLower().Contains(searchString) || t.Description.ToLower().Contains(searchString));

            }

            return result.OrderByDescending(t => t.Title);

        }
    }
}
