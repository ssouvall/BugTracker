using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly BTRolesService _rolesService;
        private readonly UserManager<BTUser> _userManager;

        public BTProjectService(ApplicationDbContext context, BTRolesService rolesService, UserManager<BTUser> userManager)
        {
            _context = context;
            _rolesService = rolesService;
            _userManager = userManager;
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            Project project = await _context.Project
                               .Include(p => p.Members)
                               .FirstOrDefaultAsync(p => p.Id == projectId);

            try
            {
                
                foreach (BTUser member in project.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, Roles.ProjectManager.ToString()))
                    {
                        await AddUserToProjectAsync(member.Id, project.Id);
                    }
                }
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    Project project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);
                    
                    if (!await IsUserOnProject(userId, projectId))
                    {
                        try
                        {
                            project.Members.Add(user);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error Adding user to project. --> {ex.Message}");
                return false;
            }
            

        }

        public async Task<List<Project>> GetAllProjectsByCompany(int companyId)
        {
            return await _context.Project.Where(p => p.CompanyId == companyId).ToListAsync();
        }

        public async Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName)
        {
            int priorityId = await LookupProjectPriorityId(priorityName);
            return await _context.Project.Where(p => p.CompanyId == companyId && p.ProjectPriorityId == priorityId).ToListAsync(); 
        }

        public async Task<List<Project>> GetArchivedProjectsByCompany(int companyId)
        {
            List<Project> archivedProjects = new();
            List<Project> allProjects = new();

            allProjects = await GetAllProjectsByCompany(companyId);
            archivedProjects = allProjects.Where(p => p.Archived == true).ToList();

            return archivedProjects;

        }

        public async Task<List<BTUser>> GetMembersWithoutPMAsync(int projectId)
        {
            List<BTUser> developers = await GetProjectMembersByRoleAsync(projectId, "Developer"); 
            List<BTUser> submitters = await GetProjectMembersByRoleAsync(projectId, "Submitter");
            List<BTUser> admins = await GetProjectMembersByRoleAsync(projectId, "Admin");

            List<BTUser> teamMembers = developers.Concat(submitters).Concat(admins).ToList();
            return teamMembers;
        }

        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            Project project = await _context.Project
                                .Include(p => p.Members)
                                .FirstOrDefaultAsync(p => p.Id == projectId);

            foreach(BTUser member in project?.Members)
            {
                if(await _rolesService.IsUserInRoleAsync(member, "ProjectManager"))
                {
                    return member;

                }
            }
            return null;
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            Project project = await _context.Project
                                .Include(p => p.Members)
                                .FirstOrDefaultAsync(p => p.Id == projectId);

            List<BTUser> members = new();
            
            foreach (BTUser member in project.Members)
            {
                if (await _rolesService.IsUserInRoleAsync(member, role))
                {
                    members.Add(member);
                }
            }
            return members;
        }

        public async Task<bool> IsUserOnProject(string userId, int projectId)
        {
            Project project = await _context.Project
                    .FirstOrDefaultAsync(u => u.Id == projectId);

            bool result = project.Members.Any(u => u.Id == userId);
            return result;
        }

        public async Task<List<Project>> ListUserProjectsAsync(string userId)
        {
            try
            {
                List<Project> userProjects = (await _context.Users
                .Include(u => u.Projects)
                    .ThenInclude(p => p.Company)
                .Include(u => u.Projects)
                    .ThenInclude(p => p.Members)
                .Include(u => u.Projects)
                    .ThenInclude(p => p.Tickets)
                .Include(u => u.Projects)
                    .ThenInclude(t => t.Tickets)
                        .ThenInclude(t => t.OwnerUser)
                .Include(u => u.Projects)
                    .ThenInclude(t => t.Tickets)
                        .ThenInclude(t => t.DeveloperUser)
                .Include(u => u.Projects)
                    .ThenInclude(t => t.Tickets)
                        .ThenInclude(t => t.TicketPriority)
                .Include(u => u.Projects)
                    .ThenInclude(t => t.Tickets)
                        .ThenInclude(t => t.TicketStatus)
                .Include(u => u.Projects)
                    .ThenInclude(t => t.Tickets)
                        .ThenInclude(t => t.TicketType)
                .FirstOrDefaultAsync(u => u.Id == userId)).Projects.ToList();

                return userProjects;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** . Error listing Users on project. --> {ex.Message}");
                throw;
            }


            
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            Project project = await _context.Project
                               .Include(p => p.Members)
                               .FirstOrDefaultAsync(p => p.Id == projectId);

            try
            {
                
                foreach (BTUser member in project.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, Roles.ProjectManager.ToString()))
                    {
                        await RemoveUserFromProjectAsync(member.Id, project.Id);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Project project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);
                if (await IsUserOnProject(userId, projectId))
                {
                    try
                    {
                        project.Members.Remove(btUser);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** . Error Removing User from project. --> {ex.Message}");
            }
        }

        public async Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
        {
            try
            {
                List<BTUser> members = await GetProjectMembersByRoleAsync(projectId, role);
                Project project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);
                
                foreach(BTUser btUser in members)
                {
                    try
                    {
                        project.Members.Remove(btUser);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** . Error Removing Users from project. --> {ex.Message}");

            }
        }

        public async Task<List<BTUser>> UsersNotOnProjectAsync(int projectId, int companyId)
        {
            List<BTUser> usersNotOnProject = await _context.Users.Where(u => u.Projects.All(p => p.Id != projectId) && u.CompanyId == companyId).ToListAsync();

            return usersNotOnProject;
        }

        public async Task<int> LookupProjectPriorityId(string priorityName)
        {
            return (await _context.ProjectPriority.FirstOrDefaultAsync(p => p.Name == priorityName)).Id;
        }
    }
}
