using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services.Interfaces
{
    public interface IBTTicketService
    {
        Task AssignTicketAsync(int ticketId, string userId);

        Task<BTUser> GetTicketDeveloperAsync(int ticketId);

        Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId);

        Task<List<Ticket>> GetArchivedTicketsByCompanyAsync(int companyId);

        Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName);

        Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName);

        Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName);

        Task<List<Ticket>> GetAllPMTicketsAsync(string userId);

        Task<List<Ticket>> GetAllTicketsByRoleAsync(string role, string userId);

        Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId);

        Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId);

        Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId);

        Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId);

        Task<int?> LookupTicketPriorityIdAsync(string priorityName);

        Task<int?> LookupTicketStatusIdAsync(string statusName);

        Task<int?> LookupTicketTypeIdAsync(string typeName);

        Task<List<TicketStatus>> GetTicketStatusListAsync();

    }
}
