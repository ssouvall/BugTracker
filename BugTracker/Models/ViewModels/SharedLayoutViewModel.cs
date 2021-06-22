using BugTracker.Data;
using BugTracker.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using BugTracker.Extensions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BugTracker.Models.ViewModels
{
    public class SharedLayoutViewModel
    {
        //private readonly BTNotificationService _notificationService;
        //private readonly UserManager<BTUser> _userManager;
        //private readonly ApplicationDbContext _context;

        //public SharedLayoutViewModel(BTNotificationService notificationService,
        //                            UserManager<BTUser> userManager,
        //                            ApplicationDbContext context)
        //{
        //    _notificationService = notificationService;
        //    _userManager = userManager;
        //    _context = context;
        //}
        public List<Notification> Notifications { get; set; }
    
        //public async Task OnGetAsync(BTUser btUser)
        //{
        //    List<Notification> notifications = await _notificationService.GetReceivedNotificationsAsync(btUser.Id);

        //    return notifications;

        //}
    }

}
