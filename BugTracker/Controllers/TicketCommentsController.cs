using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using BugTracker.Services.Interfaces;
using BugTracker.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;

        public TicketCommentsController(ApplicationDbContext context,
                                        UserManager<BTUser> userManager,
                                        IBTTicketService ticketService,
                                        IBTProjectService projectService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _projectService = projectService;
        }

        // GET: TicketComments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TicketComment.Include(t => t.Ticket).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TicketComments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketComment = await _context.TicketComment
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .Include(t => t.Comment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketComment == null)
            {
                return NotFound();
            }

            return View(ticketComment);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Comment,Created,TicketId,ProjectId,UserId,User")] TicketComment ticketComment)
        {
            if (ModelState.IsValid)
            {
                int companyId = User.Identity.GetCompanyId().Value;
                List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);
                Ticket ticket = tickets.FirstOrDefault(t => t.Id == ticketComment.TicketId);
                Project project = ticket.Project;
                BTUser currentUser = await _userManager.GetUserAsync(User);
                BTUser ticketOwner = ticket.OwnerUser;
                BTUser ticketDeveloper = ticket.DeveloperUser;
                BTUser projectManager = await _projectService.GetProjectManagerAsync(project.Id);

                if(!string.IsNullOrEmpty(ticketComment.Comment))
                {
                    if(currentUser.Id == ticketOwner?.Id || currentUser.Id == ticketDeveloper?.Id || currentUser.Id == projectManager?.Id || User.IsInRole("Admin"))
                    {
                        ticketComment.Created = DateTime.Now;
                        ticketComment.UserId = _userManager.GetUserId(User);
                        _context.Add(ticketComment);
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Details", "Tickets", new { Id = ticketComment.TicketId });
                    }
                    else
                    {
                        return RedirectToAction("Details", "Tickets", new { Id = ticketComment.TicketId });
                    }
                    
                }
                
                
            }
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Description", ticketComment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.UserId);
            return RedirectToAction("Details", "Tickets", new { Id = ticketComment.TicketId });
        }

        // GET: TicketComments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketComment = await _context.TicketComment.FindAsync(id);
            if (ticketComment == null)
            {
                return NotFound();
            }
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Description", ticketComment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.UserId);
            return View(ticketComment);
        }

        // POST: TicketComments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        {
            if (id != ticketComment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketComment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketCommentExists(ticketComment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketId"] = new SelectList(_context.Ticket, "Id", "Description", ticketComment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.UserId);
            return View(ticketComment);
        }

        // GET: TicketComments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketComment = await _context.TicketComment
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketComment == null)
            {
                return NotFound();
            }

            return View(ticketComment);
        }

        // POST: TicketComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketComment = await _context.TicketComment.FindAsync(id);
            _context.TicketComment.Remove(ticketComment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketCommentExists(int id)
        {
            return _context.TicketComment.Any(e => e.Id == id);
        }
    }
}
