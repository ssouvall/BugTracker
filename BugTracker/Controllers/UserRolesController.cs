﻿using BugTracker.Data;
using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Controllers
{
    [Authorize]
    public class UserRolesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTRolesService _rolesService;

        public UserRolesController(ApplicationDbContext context,
                                   UserManager<BTUser> userManager,
                                   IBTCompanyInfoService companyInfoService,
                                   IBTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
            _rolesService = rolesService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            List<BTUser> users = await _companyInfoService.GetAllMembersAsync(companyId);

            foreach (var user in users)
            {
                ManageUserRolesViewModel vm = new();
                vm.BTUser = user;
                var selected = await _rolesService.ListUserRolesAsync(user);
                vm.Roles = new MultiSelectList(_context.Roles, "Name", "Name", selected);
                model.Add(vm);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            BTUser user = _context.Users.Find(member.BTUser.Id);

            IEnumerable<string> roles = await _rolesService.ListUserRolesAsync(user);
            await _rolesService.RemoveUsersFromRolesAsync(user, roles);
            string userRole = member.SelectedRoles.FirstOrDefault();

            if(Enum.TryParse(userRole, out Roles roleValue))
            {
                await _rolesService.AddUserToRoleAsync(user, userRole);
                return RedirectToAction("ManageUserRoles");
            }

            return RedirectToAction("ManageUserRoles");
        }
    }
}
