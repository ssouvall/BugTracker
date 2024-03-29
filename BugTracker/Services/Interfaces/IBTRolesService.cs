﻿using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services.Interfaces
{
    public interface IBTRolesService
    {
        public Task<IEnumerable<string>> ListUserRolesAsync(BTUser user);
        public Task<bool> IsUserInRoleAsync(BTUser user, string roleName);
        public Task<bool> AddUserToRoleAsync(BTUser user, string roleName);
        public Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName);
        public Task<IdentityResult> RemoveUsersFromRolesAsync(BTUser user, IEnumerable<string> roles);
        public Task<List<BTUser>> UsersNotInRoleAsync(string roleName, int companyId);
        public Task<string> GetRoleNameByIdAsync(string roleId);
    }
}
