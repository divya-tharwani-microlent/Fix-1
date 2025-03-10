using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zoho_timesheet_Core.Entities;
using Zoho_timesheet_EFC;

namespace Zoho_timesheet_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        public readonly ZohoTimesheetDBContext _context;
        public CommonController(ZohoTimesheetDBContext context)
        {
            _context = context;
        }

        [HttpGet("category-list")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        [HttpGet("project-list")]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            var projects = await _context.Projects.Where(p=>p.IsActive).ToListAsync();
            return Ok(projects);
        }

        [HttpGet("projTask-list")]
        public async Task<ActionResult<IEnumerable<ProjectTask>>> GetProjectTasks_byProjectId(int ProjectId)
        {
            var projTasks = await _context.ProjectTasks.Where(p => p.IsActive && p.ProjectId == ProjectId).ToListAsync();
            return Ok(projTasks);
        }

        [HttpGet("admin-list")]
        public async Task<ActionResult<IEnumerable<Admin>>> GetAdminList()
        {
            return await _context.Admins.Where(p => p.IsActive).ToListAsync();
        }
    }
}
