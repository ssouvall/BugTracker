using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTTicketService :  IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        //private readonly IBTCompanyInfoService _companyInfoService;
        //private readonly UserManager<BTUser> _userManager;

        public BTTicketService(ApplicationDbContext context, IBTProjectService projectService)
        {
            _context = context;
            _projectService = projectService;
       //     _companyInfoService = companyInfoService;
        //    _userManager = userManager;
        }
        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            Ticket ticket = await _context.Ticket.FirstOrDefaultAsync(t => t.Id == ticketId);

            if(ticket != null)
            {
                try
                {
                    ticket.TicketStatusId = (await LookupTicketStatusIdAsync("Development")).Value;
                    ticket.DeveloperUserId = userId;
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task<List<Ticket>> GetAllPMTicketsAsync(string userId)
        {
            List<Project> projects = await _projectService.ListUserProjectsAsync(userId);
            List<Ticket> tickets = projects.SelectMany(t => t.Tickets).ToList();

            return tickets;
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
            List<Ticket> companyTickets = new();

            try
            {
                companyTickets = await _context.Project.Include(p => p.Company)
                                                                .Where(p => p.CompanyId == companyId)
                                                                .SelectMany(p => p.Tickets)
                                                                    .Include(t => t.Attachments)
                                                                    .Include(t => t.Comments)
                                                                    .Include(t => t.History)
                                                                    .Include(t => t.DeveloperUser)
                                                                    .Include(t => t.OwnerUser)
                                                                    .Include(t => t.TicketPriority)
                                                                    .Include(t => t.TicketStatus)
                                                                    .Include(t => t.TicketType)
                                                                    .Include(t => t.Project)
                                                                    .ToListAsync();
            }
            catch
            {
                throw;
            }

            return companyTickets;

        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            int priorityId = (await LookupTicketPriorityIdAsync(priorityName)).Value;
            List<Ticket> tickets = new();

            try
            {
                

                tickets = await _context.Project.Where(p => p.CompanyId == companyId)
                                                .SelectMany(p => p.Tickets)
                                                .Include(t => t.Attachments)
                                                .Include(t => t.Comments)
                                                .Include(t => t.History)
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.OwnerUser)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.Project)
                                                .Where(t => t.TicketPriorityId == priorityId)
                                                .ToListAsync();
            }
            catch
            {
                throw;
            }

            return tickets;
            
        }

        public async Task<List<Ticket>> GetAllTicketsByRoleAsync(string role, string userId)
        {
            List<Ticket> tickets = new();

            if(string.Compare(role, "Developer") == 0)
            {
                tickets = await _context.Ticket
                                    .Include(t => t.Attachments)
                                    .Include(t => t.Comments)
                                    .Include(t => t.DeveloperUser)
                                    .Include(t => t.OwnerUser)
                                    .Include(t => t.TicketPriority)
                                    .Include(t => t.TicketStatus)
                                    .Include(t => t.TicketType)
                                    .Include(t => t.Project)
                                        .ThenInclude(p => p.Members)
                                    .Include(t => t.Project)
                                        .ThenInclude(p => p.ProjectPriority)
                                    .Where(t => t.DeveloperUserId == userId).ToListAsync();
            }
            else if (string.Compare(role, "Submitter") == 0)
            {
                tickets = await _context.Ticket
                                    .Include(t => t.Attachments)
                                    .Include(t => t.Comments)
                                    .Include(t => t.DeveloperUser)
                                    .Include(t => t.OwnerUser)
                                    .Include(t => t.TicketPriority)
                                    .Include(t => t.TicketStatus)
                                    .Include(t => t.TicketType)
                                    .Include(t => t.Project)
                                        .ThenInclude(p => p.Members)
                                    .Include(t => t.Project)
                                        .ThenInclude(p => p.ProjectPriority).Where(t => t.OwnerUserId == userId).ToListAsync();

            }
            else if (string.Compare(role, "ProjectManager") == 0)
            {
                tickets = await GetAllPMTicketsAsync(userId);
            }

            return tickets;
        }

        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            int statusId = (await LookupTicketStatusIdAsync(statusName)).Value;
            List<Ticket> tickets = new();

            try
            {
               

                tickets = await _context.Project.Where(p => p.CompanyId == companyId)
                                                .SelectMany(p => p.Tickets)
                                                .Include(t => t.Attachments)
                                                .Include(t => t.Comments)
                                                .Include(t => t.History)
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.OwnerUser)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.Project)
                                                .Where(t => t.TicketStatusId == statusId)
                                                .ToListAsync();
            }
            catch
            {
                throw;
            }

            return tickets;
        }


        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            int typeId = (await LookupTicketTypeIdAsync(typeName)).Value;
            List<Ticket> tickets = new();

            try
            {
                tickets = await _context.Project.Where(p => p.CompanyId == companyId)
                                                .SelectMany(p => p.Tickets)
                                                .Include(t => t.Attachments)
                                                .Include(t => t.Comments)
                                                .Include(t => t.History)
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.OwnerUser)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.Project)
                                                .Where(t => t.TicketTypeId == typeId)
                                                .ToListAsync();
            }
            catch
            {
                throw;
            }

            return tickets;
        }

        public async Task<List<Ticket>> GetArchivedTicketsByCompanyAsync(int companyId)
        {
            try
            {
                List<Ticket> archivedTickets = await _context.Project.Where(p => p.CompanyId == companyId)
                                                                                   .SelectMany(p => p.Tickets)
                                                                                       .Include(t => t.Attachments)
                                                                                       .Include(t => t.Comments)
                                                                                       .Include(t => t.History)
                                                                                       .Include(t => t.DeveloperUser)
                                                                                       .Include(t => t.OwnerUser)
                                                                                       .Include(t => t.TicketPriority)
                                                                                       .Include(t => t.TicketStatus)
                                                                                       .Include(t => t.TicketType)
                                                                                       .Include(t => t.Project)
                                                                                       .Where(t => t.Archived == true)
                                                                                       .ToListAsync();
                return archivedTickets;
            }
            catch
            {
                throw;
            }

        }
           

        public async Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId)
        {
            List<Ticket> tickets = new();

            tickets = (await GetAllTicketsByRoleAsync(role, userId)).Where(t => t.ProjectId == projectId).ToList();

            return tickets;

        }

        public async Task<BTUser> GetTicketDeveloperAsync(int ticketId)
        {
            BTUser developer = new();

            Ticket ticket = await _context.Ticket.Include(t => t.DeveloperUser).FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket?.DeveloperUserId != null)
            {
                developer = ticket.DeveloperUser;
            }

            return developer;
        }

        public async Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            tickets = (await GetAllTicketsByStatusAsync(companyId, statusName))
                .Where(t => t.ProjectId == projectId).ToList();

            return tickets;
        }

        public async Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            tickets = (await GetAllTicketsByPriorityAsync(companyId, priorityName))
                .Where(t => t.ProjectId == projectId).ToList();

            return tickets;
        }

        public async Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            tickets = (await GetAllTicketsByTypeAsync(companyId, typeName))
                .Where(t => t.ProjectId == projectId).ToList();

            return tickets;
        }

        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            try
            {
                int priorityId = (await _context.TicketPriority.FirstOrDefaultAsync(t => t.Name == priorityName)).Id;
                return priorityId;
            }
            catch
            {
                throw;
            }
            
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            try
            {
                int statusId = (await _context.TicketStatus.FirstOrDefaultAsync(t => t.Name == statusName)).Id;
                return statusId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            try
            {
                int typeId = (await _context.TicketType.FirstOrDefaultAsync(t => t.Name == typeName)).Id;
                return typeId;
            }
            catch
            {
                throw;
            }
           
        }

        public async Task<List<TicketStatus>> GetTicketStatusListAsync()
        {
            List<TicketStatus> ticketStatusNames = await _context.TicketStatus.ToListAsync();
            return ticketStatusNames;
        }
    }
}
