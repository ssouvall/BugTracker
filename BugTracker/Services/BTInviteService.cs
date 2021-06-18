using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTInviteService : IBTInviteService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IEmailSender _emailService;

        public BTInviteService(ApplicationDbContext context, IBTProjectService projectService,
                                  IEmailSender emailService
                                  )
        {
            _context = context;
            _projectService = projectService;
            _emailService = emailService;
        }

        public async Task<Invite> GetInviteAsync(Guid token, string email)
        {
            Invite invite = await _context.Invite.Include(i => i.Company)
                                            .Include(i => i.Project)
                                            .Include(i => i.Invitor)
                                            .FirstOrDefaultAsync(i => i.CompanyToken == token && i.InviteeEmail == email);

            return invite;
        }

        public async Task<Invite> GetInviteAsync(int id)
        {
            Invite invite = await _context.Invite.Include(i => i.Company)
                                            .Include(i => i.Project)
                                            .Include(i => i.Invitor)
                                            .FirstOrDefaultAsync(i => i.Id == id);

            return invite;
        }

        public async Task<bool> AnyInviteAsync(Guid token, string email)
        {
            return await _context.Invite.AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email && i.IsValid == true);
        }

        public async Task<bool> AcceptInviteAsync(Guid? code, string userId)
        {

            Invite invite = await _context.Invite.FirstOrDefaultAsync(i => i.CompanyToken == code);

            if (invite == null)
            {
                return false;
            }

            try
            {
                //invite.IsValid = false;
                invite.InviteeId = userId;
                _context.SaveChanges();

                return true;
            }
            catch
            { throw; }


        }

        public async Task<bool> ValidateInviteCodeAsync(Guid? code)
        {
            if (code == null)
            {
                return false;
            }

            var invite = await _context.Invite.FirstOrDefaultAsync(i => i.CompanyToken == code);

            if ((DateTime.Now - (await _context.Invite.FirstOrDefaultAsync(i => i.CompanyToken == code)).InviteDate).TotalDays <= 7)
            {

                bool result = (await _context.Invite.FirstOrDefaultAsync(i => i.CompanyToken == code)).IsValid;

                return result;
            }
            else
            {
                return false;
            }

        }

    }
}
