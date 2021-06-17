using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTNotificationService : IBTNotificationsService
    {

        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IBTCompanyInfoService _companyInfoService;

        public BTNotificationService(ApplicationDbContext context,
                                       IEmailSender emailSender,
                                       IBTCompanyInfoService companyInfoService)
        {
            _context = context;
            _emailSender = emailSender;
            _companyInfoService = companyInfoService;
        }

        public async Task SaveNotificationAsync(Notification notification)
        {
            try
            {
                await _context.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }
        
        public async Task AdminsNotificationAsync(Notification notification, int companyId)
        {
            try
            {
                List<BTUser> admins = await _companyInfoService.GetMembersInRoleAsync("Admin", companyId);

                foreach(BTUser btUser in admins)
                {
                    notification.RecipientId = btUser.Id;

                    await EmailNotificationAsync(notification, notification.Title);
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task MembersNotificationAsync(Notification notification, List<BTUser> members)
        {
            try
            {
                foreach (BTUser btUser in members)
                {
                    notification.RecipientId = btUser.Id;

                    //await SaveNotificationsAsync(notification);
                    await EmailNotificationAsync(notification, notification.Title);

                }
            }
            catch
            {
                throw;
            }
        }

        public async Task EmailNotificationAsync(Notification notification, string emailSubject)
        {
            BTUser btUser = await _context.Users.FindAsync(notification.RecipientId);

            //Send Email
            string btUserEmail = btUser.Email;
            string message = notification.Message;

            try
            {
                await _emailSender.SendEmailAsync(btUserEmail, emailSubject, message);
            }
            catch
            {
                throw;
            }

        }

        public Task SMSNotificationAsync(string phone, Notification notification)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Notification>> GetReceivedNotificationsAsync(string userId)
        {
            List<Notification> notifications = await _context.Notification
                                                    .Include(n => n.Recipient)
                                                    .Include(n => n.Sender)
                                                    .Include(n => n.Ticket)
                                                        .ThenInclude(t => t.Project)
                                                    .Where(n => n.RecipientId == userId).ToListAsync();
            return notifications;
        }

        public async Task<List<Notification>> GetSentNotificationsAsync(string userId)
        {
            List<Notification> notifications = await _context.Notification
                                                                .Include(n => n.Recipient)
                                                                .Include(n => n.Sender)
                                                                .Include(n => n.Ticket)
                                                                    .ThenInclude(t => t.Project)
                                                                .Where(n => n.SenderId == userId).ToListAsync();
            return notifications;

        }
    }
}
