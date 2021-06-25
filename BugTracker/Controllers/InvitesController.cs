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
using Microsoft.AspNetCore.DataProtection;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using BugTracker.Models.ViewModels;
using BugTracker.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace BugTracker.Controllers
{
    [Authorize]
    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IDataProtector _protector;
        private readonly IBTProjectService _projectService;
        private readonly IEmailSender _emailService;
        private readonly IBTInviteService _inviteService;

        public InvitesController(ApplicationDbContext context,
                              UserManager<BTUser> userManager,
                              IDataProtectionProvider dataProtectionProvider,
                              IBTProjectService projectService,
                              IEmailSender emailService,
                              IBTInviteService inviteService)
        {
            _context = context;
            _userManager = userManager;
            _protector = dataProtectionProvider.CreateProtector("CF.RockwellTracker.21");
            _projectService = projectService;
            _emailService = emailService;
            _inviteService = inviteService;
        }

        // GET: Invites
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invite.Include(i => i.Company).Include(i => i.Invitee).Include(i => i.Invitor).Include(i => i.Project);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invite
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // GET: Invites/Create
        public async Task<IActionResult> Create()
        {
            InviteViewModel model = new();

            if (User.IsInRole("Admin"))
            {
                model.ProjectsList = new SelectList(_context.Project, "Id", "Name");
            }
            else if (User.IsInRole("ProjectManager"))
            {
                string userId = _userManager.GetUserId(User);
                List<Project> projects = await _projectService.ListUserProjectsAsync(userId);
                model.ProjectsList = new SelectList(projects, "Id", "Name");
            }
            TempData["Status"] = "";
            return View(model);
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(InviteViewModel viewModel)
        {
            var companyId = User.Identity.GetCompanyId();

            Guid guid = Guid.NewGuid();

            var token = _protector.Protect(guid.ToString());
            var email = _protector.Protect(viewModel.Email);

            var callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email }, protocol: Request.Scheme);

            var body = "Please join my Company." + Environment.NewLine + "Please click the following link to join <a href=\"" + callbackUrl + "\">COLLABORATE</a>";
            var destination = viewModel.Email;
            var subject = "Company Invite";


            await _emailService.SendEmailAsync(destination, subject, body);

            //Create record in the Invites table
            Invite model = new()
            {
                InviteeEmail = viewModel.Email,
                InviteeFirstName = viewModel.FirstName,
                InviteeLastName = viewModel.LastName,
                CompanyToken = guid,
                CompanyId = companyId.Value,
                ProjectId = viewModel.ProjectId,
                InviteDate = DateTimeOffset.Now,
                InvitorId = _userManager.GetUserId(User),
                IsValid = true
            };

            _context.Invite.Add(model);
            _context.SaveChanges();

            TempData["Status"] = "Success";
            return RedirectToAction("Index", "Home");
        }

        // GET: Invites/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invite.FindAsync(id);
            if (invite == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            ViewData["ProjectId"] = new SelectList(_context.Set<Project>(), "Id", "Name", invite.ProjectId);
            return View(invite);
        }

        // POST: Invites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InviteDate,CompanyToken,CompanyId,ProjectId,InvitorId,InviteeId,InviteeEmail,InviteeFirstName,InviteeLastName,IsValid")] Invite invite)
        {
            if (id != invite.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InviteExists(invite.Id))
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
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            ViewData["ProjectId"] = new SelectList(_context.Set<Project>(), "Id", "Name", invite.ProjectId);
            return View(invite);
        }

        // GET: Invites/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invite
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // POST: Invites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invite = await _context.Invite.FindAsync(id);
            _context.Invite.Remove(invite);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ProcessInvite(string token, string email)
        {
            if (token == null)
            {
                return NotFound();
            }

            Guid companyToken = Guid.Parse(_protector.Unprotect(token));
            string inviteeEmail = _protector.Unprotect(email);

            //Use InviteService to validate invite code 
            Invite invite = await _inviteService.GetInviteAsync(companyToken, inviteeEmail);

            if (invite != null)
            {
                return View(invite);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessInvite(Invite invite)
        {
            return RedirectToPage("RegisterByInvite", new { invite });
        }

        private bool InviteExists(int id)
        {
            return _context.Invite.Any(e => e.Id == id);
        }
    }
}
