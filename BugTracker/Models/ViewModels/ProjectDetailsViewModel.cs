﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace BugTracker.Models.ViewModels
{
    public class ProjectDetailsViewModel
    {
        public Project Project { get; set; } = new();
        public SelectList ProjectManagers { get; set; }
        public BTUser ProjectManager { get; set; }
        public BTUser CurrentUser { get; set; }
        public IPagedList<Ticket> Tickets { get; set; }
    }
}
