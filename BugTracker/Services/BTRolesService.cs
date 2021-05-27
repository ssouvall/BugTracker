using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTRolesService : IBTRolesService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _infoService;

        //create a constructor
        public BTRolesService(ApplicationDbContext context, 
            RoleManager<IdentityRole> roleManager,
            UserManager<BTUser> userManager,
            IBTCompanyInfoService infoService)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _infoService = infoService;
        }

        public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        {
            bool result = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
            return result;
        }

        public async Task<string> GetRoleNameByIdAsync(string roleId)
        {
            IdentityRole role = _context.Roles.Find(roleId);
            string result = await _roleManager.GetRoleNameAsync(role);
            return result;
        }

        //figure out if this user is in a certain role
        public async Task<bool> IsUserInRoleAsync(BTUser user, string roleName)
        {
            bool result = await _userManager.IsInRoleAsync(user, roleName);
            return result;
        }

        public async Task<IEnumerable<string>> ListUserRolesAsync(BTUser user)
        {
            IEnumerable<string> result = await _userManager.GetRolesAsync(user);
            return result;
        }
            
        public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            bool result = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
            return result;
        }

        public async Task<IdentityResult> RemoveUsersFromRolesAsync(BTUser user, IEnumerable<string> roles)
        {
            IdentityResult result = await _userManager.RemoveFromRolesAsync(user, roles);
            return result;
        }

        public async Task<List<BTUser>> UsersNotInRoleAsync(string roleName, int companyId)
        {
            List<BTUser> usersNotInRole = new();
            try 
            { 
                //Will modify for multi-tenancy
                foreach(BTUser user in await _infoService.GetAllMembersAsync(companyId))
                    {
                        if(!await IsUserInRoleAsync(user, roleName))
                        {
                            usersNotInRole.Add(user);   

                        }
                    } 
            } 
            catch
            {
                throw;
            }

            return usersNotInRole;
        }
    }
}
