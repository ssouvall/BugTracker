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

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyInfoService _companyInfoService;

        public HomeController(ILogger<HomeController> logger,
                              UserManager<BTUser> userManager,
                              IBTTicketService ticketService,
                              IBTProjectService projectService,
                              IBTCompanyInfoService companyInfoService)
        {
            _logger = logger;
            _userManager = userManager;
            _ticketService = ticketService;
            _projectService = projectService;
            _companyInfoService = companyInfoService;
        }

        public async Task<IActionResult> Index()
        {
            //string userId = _userManager.GetUserId(User);
            if (User.Identity.IsAuthenticated)
            {
                int companyId = User.Identity.GetCompanyId().Value;
                var members = await _companyInfoService.GetAllMembersAsync(companyId);
                var tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);
                var projects = await _projectService.GetAllProjectsByCompany(companyId);
                var model = new DashboardViewModel()
                
                {
                    Tickets = tickets,
                    Projects = projects,
                    Users = members,
                };
                return View(model);
            }
            else
            {
                return View();
            }
            
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
