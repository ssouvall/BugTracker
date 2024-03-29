﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using BugTracker.Extensions;
using BugTracker.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using X.PagedList;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyInfoService _bTCompanyInfoService;
        private readonly IBTTicketService _ticketService;
        private readonly UserManager<BTUser> _userManager;

        public ProjectsController(ApplicationDbContext context,
                                    IBTProjectService projectService,
                                    IBTCompanyInfoService bTCompanyInfoService,
                                    IBTTicketService ticketService,
                                    UserManager<BTUser> userManager)
        {
            _context = context;
            _projectService = projectService;
            _bTCompanyInfoService = bTCompanyInfoService;
            _ticketService = ticketService;
            _userManager = userManager;
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? page, int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProjectDetailsViewModel model = new();
            
            var pageNumber = page ?? 1;
            var pageSize = 15;

            var project = await _context.Project
                .Include(p => p.Members)
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .Include(p => p.Tickets)
                    .ThenInclude(t => t.TicketPriority)
                .Include(p => p.Tickets)
                    .ThenInclude(t => t.TicketStatus)
                .FirstOrDefaultAsync(m => m.Id == id);

            List<Ticket> pageTickets = project.Tickets.ToList();

            if (project == null)
            {
                return NotFound();
            }

            BTUser projectManager = await _projectService.GetProjectManagerAsync(project.Id);
            BTUser currentUser = await _userManager.GetUserAsync(User);

            if (projectManager is not null)
            {
                
                model.Project = project;
                model.ProjectManager = projectManager;
                model.CurrentUser = currentUser;
                model.Tickets = await pageTickets?.ToPagedListAsync(pageNumber, pageSize);

                return View(model);
            }
            else
            {

                model.Project = project;
                model.Tickets = await pageTickets?.ToPagedListAsync(pageNumber, pageSize);

                return View(model);
            }
            
        }

        [Authorize(Roles= "Admin,ProjectManager")]
        // GET: Projects/Create
        public IActionResult Create()
        {
            
            //ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name");
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate,ProjectPriorityId,FileName,FileData,FileContentType")] Project project)
        {   
            if (ModelState.IsValid)
            {
                project.CompanyId = User.Identity.GetCompanyId().Value;
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Projects", new { id = project.Id });
            }
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        [Authorize(Roles = "Admin,ProjectManager")]
        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;
            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,StartDate,EndDate,ProjectPriorityId,FileName,FileData,FileContentType,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Projects", new { id = project.Id });
            }
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }


        //[Authorize(Roles = "Admin,ProjectManager")]
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignUsers(int id)
        {
            ProjectMembersViewModel model = new();

            //get companyID
            int companyId = User.Identity.GetCompanyId().Value;

            Project project = (await _projectService.GetAllProjectsByCompany(companyId))
                                        .FirstOrDefault(p => p.Id == id);

            model.Project = project;
            List<BTUser> developers = await _bTCompanyInfoService.GetMembersInRoleAsync(Roles.Developer.ToString(), companyId);
            List<BTUser> submitters = await _bTCompanyInfoService.GetMembersInRoleAsync(Roles.Submitter.ToString(), companyId);

            List<BTUser> users = developers.Concat(submitters).ToList();
            if(project.Members is not null) 
            { 
            List<string> members = project.Members.Select(m => m.Id).ToList();
            model.Users = new MultiSelectList(users, "Id", "FullName", members);
            return View(model);
            }
            else
            {
                return View(model);
            }
            
        }
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUsers(ProjectMembersViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedUsers != null)
                {

                    List<string> memberIds = (await _projectService.GetMembersWithoutPMAsync(model.Project.Id))
                                                                    .Select(m => m.Id).ToList();

                    foreach (string id in memberIds)
                    {
                        await _projectService.RemoveUserFromProjectAsync(id, model.Project.Id);
                    }

                    foreach (string id in model.SelectedUsers)
                    {
                        await _projectService.AddUserToProjectAsync(id, model.Project.Id);
                    }
                    return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
                }
                else
                {
                    //send error message
                }
            }
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AssignProjectManager(int id)
        {
            ProjectManagerViewModel model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            Project project = (await _projectService.GetAllProjectsByCompany(companyId))
                                        .FirstOrDefault(p => p.Id == id);

            model.Project = project;

            List<BTUser> users = new();

            try
            {
                List<BTUser> projectManagers = await _bTCompanyInfoService.GetMembersInRoleAsync(Roles.ProjectManager.ToString(), companyId);
                users = projectManagers;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error getting project managers - {ex.Message}");
                throw;
            }


            model.Users = new SelectList(users, "Id", "FullName", users);
            BTUser assignedProjectManager = await _projectService.GetProjectManagerAsync(project.Id);
            
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectManager(ProjectManagerViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedUser != null)
                {
                    await _projectService.AddProjectManagerAsync(model.SelectedUser, model.Project.Id);

                    return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
                }
                else
                {
                    return View();
                }
            }
            return View(model);

        }

        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AllProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var members = await _bTCompanyInfoService.GetAllMembersAsync(companyId);
            var projects = await _projectService.GetAllProjectsByCompany(companyId);
            var tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);
            var model = new DashboardViewModel()

            {
                Tickets = tickets,
                Projects = projects,
                Users = members,
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MyProjects()
        {
            BTUser user = await _userManager.GetUserAsync(User);

            List<Project> myProjects = await _projectService.ListUserProjectsAsync(user.Id);

            return View(myProjects);
        }
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            List<Project> projects = await _projectService.GetArchivedProjectsByCompany(companyId);
            return View(projects);
        }

        //GET: Projects/Delete/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }
        [Authorize(Roles = "Admin,ProjectManager")]
        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Project.FindAsync(id);
            project.Archived = true;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        private bool ProjectExists(int id)
        {
            return _context.Project.Any(e => e.Id == id);
        }
    }
}
