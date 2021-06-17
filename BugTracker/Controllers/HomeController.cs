using BugTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BugTracker.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using BugTracker.Services.Interfaces;
using BugTracker.Data;
using BugTracker.Extensions;
using Microsoft.AspNetCore.Authorization;
using System.Drawing;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTRolesService _rolesService;

        public HomeController(ILogger<HomeController> logger,
                              UserManager<BTUser> userManager,
                              IBTTicketService ticketService,
                              IBTProjectService projectService,
                              IBTCompanyInfoService companyInfoService,
                              IBTRolesService rolesService)
        {
            _logger = logger;
            _userManager = userManager;
            _ticketService = ticketService;
            _projectService = projectService;
            _companyInfoService = companyInfoService;
            _rolesService = rolesService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            //string userId = _userManager.GetUserId(User);
            if (User.Identity.IsAuthenticated)
            {
                BTUser user = await _userManager.GetUserAsync(User);
                int companyId = User.Identity.GetCompanyId().Value;
                var company = await _companyInfoService.GetCompanyInfoByIdAsync(companyId);
                var members = await _companyInfoService.GetAllMembersAsync(companyId);
                var tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);
                var projects = await _projectService.GetAllProjectsByCompany(companyId);
                var developers = await _companyInfoService.GetMembersInRoleAsync("Developer", companyId);
                var submitters = await _companyInfoService.GetMembersInRoleAsync("Submitter", companyId);
                var projectManagers = await _companyInfoService.GetMembersInRoleAsync("ProjectManager", companyId);

                var model = new DashboardViewModel()
                {
                    Company = company,
                    Tickets = tickets,
                    Projects = projects,
                    Users = members,
                    Developers = developers,
                    Submitters = submitters,
                    ProjectManagers = projectManagers,
                    User = user
                };
                return View(model);
            }
            else
            {
                return View();
            }
            
        }

        public IActionResult Landing()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> PieChartMethod()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "ProjectName", "TicketCount" });

            foreach (Project prj in projects)
            {
                chartData.Add(new object[] { prj.Name, prj.Tickets.Count() });
            }

            return Json(chartData);
        }

        [HttpPost]
        public async Task<JsonResult> TicketStatusChartMethod()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> newTickets = await _ticketService.GetAllTicketsByStatusAsync(companyId, "New");
            List<Ticket> unassignedTickets = await _ticketService.GetAllTicketsByStatusAsync(companyId, "Unassigned");
            List<Ticket> devTickets = await _ticketService.GetAllTicketsByStatusAsync(companyId, "Development");
            List<Ticket> testingTickets = await _ticketService.GetAllTicketsByStatusAsync(companyId, "Testing");
            List<Ticket> resolvedTickets = await _ticketService.GetAllTicketsByStatusAsync(companyId, "Resolved");
            List<Ticket> archivedTickets = await _ticketService.GetAllTicketsByStatusAsync(companyId, "Archived");

            List<object> chartData = new();
            chartData.Add(new object[] { "TicketStatusName", "TicketCount" });

            chartData.Add(new object[] { "New", newTickets.Count() });
            chartData.Add(new object[] { "Unassigned", unassignedTickets.Count() });
            chartData.Add(new object[] { "Development", devTickets.Count() });
            chartData.Add(new object[] { "Testing", testingTickets.Count() });
            chartData.Add(new object[] { "Resolved", resolvedTickets.Count() });
            chartData.Add(new object[] { "Archived", archivedTickets.Count() });
            

            return Json(chartData);
        }

        [HttpPost]
        public async Task<JsonResult> TicketTypeChartMethod()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> newDev = await _ticketService.GetAllTicketsByTypeAsync(companyId, "New Development");
            List<Ticket> runTimeTickets = await _ticketService.GetAllTicketsByTypeAsync(companyId, "Runtime");
            List<Ticket> uiTickets = await _ticketService.GetAllTicketsByTypeAsync(companyId, "UI");
            List<Ticket> maintenanceTickets = await _ticketService.GetAllTicketsByTypeAsync(companyId, "Maintenance");

            List<object> chartData = new();
            chartData.Add(new object[] { "TicketTypeName", "TicketCount" });

            chartData.Add(new object[] { "New Development", newDev.Count() });
            chartData.Add(new object[] { "Runtime", runTimeTickets.Count() });
            chartData.Add(new object[] { "UI", uiTickets.Count() });
            chartData.Add(new object[] { "Maintenance", maintenanceTickets.Count() });


            return Json(chartData);
        }

        [HttpPost]
        public async Task<JsonResult> TicketPriorityChartMethod()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> lowPriority = await _ticketService.GetAllTicketsByPriorityAsync(companyId, "Low");
            List<Ticket> mediumPriority = await _ticketService.GetAllTicketsByPriorityAsync(companyId, "Medium");
            List<Ticket> highPriority = await _ticketService.GetAllTicketsByPriorityAsync(companyId, "High");
            List<Ticket> urgentPriority = await _ticketService.GetAllTicketsByPriorityAsync(companyId, "Urgent");

            List<object> chartData = new();
            chartData.Add(new object[] { "TicketPriorityName", "TicketCount" });

            chartData.Add(new object[] { "Low", lowPriority.Count() });
            chartData.Add(new object[] { "Medium", mediumPriority.Count() });
            chartData.Add(new object[] { "High", highPriority.Count() });
            chartData.Add(new object[] { "Urgent", urgentPriority.Count() });


            return Json(chartData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
