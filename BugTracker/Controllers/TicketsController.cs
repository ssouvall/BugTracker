using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using BugTracker.Models.ViewModels;
using BugTracker.Extensions;
using Microsoft.AspNetCore.Identity;
using System.IO;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;

namespace BugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTTicketService _ticketService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyInfoService _infoService;
        private readonly IBTHistoryService _historyService;
        private readonly IBTNotificationsService _notificationsService;

        public TicketsController(ApplicationDbContext context,
                                 IBTTicketService ticketService,
                                 UserManager<BTUser> userManager,
                                 IBTProjectService projectService,
                                 IBTCompanyInfoService infoService,
                                 IBTHistoryService historyService,
                                 IBTNotificationsService notificationsService)
        {
            _context = context;
            _ticketService = ticketService;
            _userManager = userManager;
            _projectService = projectService;
            _infoService = infoService;
            _historyService = historyService;
            _notificationsService = notificationsService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var applicationDbContext = _context.Ticket.Include(t => t.DeveloperUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View(await applicationDbContext.ToPagedListAsync(pageNumber, pageSize));
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.Attachments)
                .Include(t => t.Notifications)
                .Include(t => t.History)
                    .ThenInclude(t => t.User)
                .Include(t => t.Comments)
                    .ThenInclude(t => t.User)
                .Include(t => t.Comments)
                    .ThenInclude(t => t.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            //get current user
            BTUser btUser = await _userManager.GetUserAsync(User);
            //get current user's company
            int companyId = User.Identity.GetCompanyId().Value;

            if (User.IsInRole("Admin"))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.ListUserProjectsAsync(btUser.Id), "Id", "Name");
            }
            


            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketPriorityId,TicketTypeId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                BTUser btUser = await _userManager.GetUserAsync(User);
                int companyId = User.Identity.GetCompanyId().Value;

                ticket.Created = DateTimeOffset.Now;
                string userId = _userManager.GetUserId(User);
                ticket.OwnerUserId = userId;

                ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync("New")).Value;

                await _context.AddAsync(ticket);
                await _context.SaveChangesAsync();

                #region Add History
                //Add history
                Ticket newTicket = await _context.Ticket
                            .Include(t => t.TicketPriority)
                            .Include(t => t.TicketStatus)
                            .Include(t => t.TicketType)
                            .Include(t => t.Project)
                            .Include(t => t.DeveloperUser)
                            .AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticket.Id);

                await _historyService.AddHistory(null, newTicket, btUser.Id);
                #endregion

                #region Notification
                BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                Notification notification = new()
                    {
                        TicketId = ticket.Id,
                        Title = "New Ticket",
                        Message = $"New Ticket: {ticket?.Title}, was created by {btUser?.FullName}",
                        Created = DateTimeOffset.Now,
                        SenderId = btUser?.Id,
                        RecipientId = projectManager?.Id
                    };
                
                if (projectManager != null)
                {
                   await _notificationsService.SaveNotificationAsync(notification);
                    await _notificationsService.EmailNotificationAsync(notification, "New Ticket Added");
                }
                else
                {
                    await _notificationsService.AdminsNotificationAsync(notification, companyId);
                }
                #endregion
                
                
                return RedirectToAction("Details", "Projects", new { id = ticket.ProjectId });
            }

            return RedirectToAction("Create");
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }
            
            var ticket = await _context.Ticket.FindAsync(id);
            string ticketDeveloperId = (await _ticketService.GetTicketDeveloperAsync(ticket.Id)).Id;
            string currentUserId = (await _userManager.GetUserAsync(User)).Id;
            
            if (ticket == null)
            {
                return NotFound();
            }

            if (currentUserId == ticketDeveloperId || currentUserId == ticket.OwnerUser.Id)
            { 
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.OwnerUserId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Id", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Id", ticket.TicketTypeId);
            return View(ticket);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedDate,ProjectId,TicketPriorityId,TicketTypeId,TicketStatusId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            
            if (ModelState.IsValid)
            {

                int companyId = User.Identity.GetCompanyId().Value;
                BTUser btUser = await _userManager.GetUserAsync(User);
                BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                BTUser ticketDeveloper = await _ticketService.GetTicketDeveloperAsync(ticket.Id);

                Ticket oldTicket = await _context.Ticket
                                            .Include(t => t.TicketPriority)
                                            .Include(t => t.TicketStatus)
                                            .Include(t => t.TicketType)
                                            .Include(t => t.DeveloperUser)
                                            .AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticket.Id);

                if(btUser.Id == ticketDeveloper.Id || btUser.Id == ticket.OwnerUser.Id)
                {
                    try
                    {
                        ticket.Updated = DateTimeOffset.Now;
                        _context.Update(ticket);
                        await _context.SaveChangesAsync();

                        Notification notification = new()
                        {
                            TicketId = ticket.Id,
                            Title = "New Ticket",
                            Message = $"New Ticket: {ticket?.Title}, was created by {btUser?.FullName}",
                            Created = DateTimeOffset.Now,
                            SenderId = btUser?.Id,
                            RecipientId = projectManager?.Id
                        };

                        if (projectManager != null)
                        {
                            await _notificationsService.SaveNotificationAsync(notification);

                        }
                        else
                        {
                            await _notificationsService.AdminsNotificationAsync(notification, companyId);
                        }

                        if (ticket.DeveloperUserId != null)
                        {
                            notification = new()
                            {
                                TicketId = ticket.Id,
                                Title = "A ticket assigned to you has been modified",
                                Message = $"Ticket: [{ticket.Id}]:{ticket.Title} updated by {btUser?.FullName}",
                                Created = DateTimeOffset.Now,
                                SenderId = btUser?.Id,
                                RecipientId = ticket.DeveloperUserId
                            };
                            await _notificationsService.SaveNotificationAsync(notification);
                            await _notificationsService.EmailNotificationAsync(notification, "A Ticket Has Been Edited");
                        }

                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TicketExists(ticket.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                

                    //Add a History
                    Ticket newTicket = await _context.Ticket
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.DeveloperUser)
                                                .AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticket.Id);

                    await _historyService.AddHistory(oldTicket, newTicket, btUser.Id);

                    return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
                ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.OwnerUserId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Id", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Id", ticket.TicketTypeId);
            return View(ticket);
        }

        //create an action called AssignTicket and a corresponding view
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignTicket(int? ticketId)
        {
            if (!ticketId.HasValue)
            {
                return NotFound();
            }

            AssignDeveloperViewModel model = new();
            int companyId = User.Identity.GetCompanyId().Value;

            model.ticket = (await _ticketService.GetAllTicketsByCompanyAsync(companyId))
                                                .FirstOrDefault(t => t.Id == ticketId);
            List<BTUser> developers = await _infoService.GetMembersInRoleAsync("Developer", companyId);
            model.Developers = new SelectList(developers, "Id", "FullName");
            return View(model);
        }
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicket (AssignDeveloperViewModel model)
        {
            if(!string.IsNullOrEmpty(model.DeveloperId))
            {
                int companyId = User.Identity.GetCompanyId().Value;

                //current user, developer, project manager
                BTUser currentUser = await _userManager.GetUserAsync(User);
                BTUser developer = (await _infoService.GetAllMembersAsync(companyId)).FirstOrDefault(m => m.Id == model.DeveloperId);
                BTUser projectManager = await _projectService.GetProjectManagerAsync(model.ticket.ProjectId);

                Ticket oldTicket = await _context.Ticket
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.Project)
                                                .Include(t => t.DeveloperUser)
                                                .AsNoTracking().FirstOrDefaultAsync(t => t.Id == model.ticket.Id);
                await _ticketService.AssignTicketAsync(model.ticket.Id, model.DeveloperId);

                Ticket newTicket = await _context.Ticket
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .Include(t => t.Project)
                                                .Include(t => t.DeveloperUser)
                                                .AsNoTracking().FirstOrDefaultAsync(t => t.Id == model.ticket.Id);

                await _historyService.AddHistory(oldTicket, newTicket, currentUser.Id);
            }

            return RedirectToAction("Details", "Tickets", new { id = model.ticket.Id });
        }

        public IActionResult ShowFile(int id)
        {
            TicketAttachment ticketAttachment = _context.TicketAttachment.Find(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        [HttpGet]
        public async Task<IActionResult> AllTickets(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            int companyId = User.Identity.GetCompanyId().Value;
            var applicationDbContext = await _ticketService.GetAllTicketsByCompanyAsync(companyId);

            return View(await applicationDbContext.ToPagedListAsync(pageNumber, pageSize));
        }

        [HttpGet]
        public async Task<IActionResult> MyTickets(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            string userId = _userManager.GetUserId(User);
            var devTickets = await _ticketService.GetAllTicketsByRoleAsync("Developer", userId);
            var subTickets = await _ticketService.GetAllTicketsByRoleAsync("Submitter", userId);
            var model = new MyTicketsViewModel()
            {
                DevTickets = await devTickets.ToPagedListAsync(pageNumber, pageSize),
                SubmittedTickets = await subTickets.ToPagedListAsync(pageNumber, pageSize)
            };
            return View(model);

        }

        [HttpGet]
        public async Task<IActionResult> ArchivedTickets(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            int companyId = User.Identity.GetCompanyId().Value;
            var applicationDbContext = await _ticketService.GetAllTicketsByCompanyAsync(companyId);
            var archived = applicationDbContext.Where(t => t.TicketStatus.Name == "Archived").ToPagedListAsync(pageNumber, pageSize);

            return View(await archived);

        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            _context.Ticket.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.Id == id);
        }
    }
}
