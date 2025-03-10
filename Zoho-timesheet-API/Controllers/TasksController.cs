using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Serilog;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Zoho_timesheet_API.Enums;
using Zoho_timesheet_API.Models;
using Zoho_timesheet_API.Services;
using Zoho_timesheet_Core.Entities;
using Zoho_timesheet_EFC;
using JsonSerializer = System.Text.Json.JsonSerializer;

[Route("api/[controller]")]
[ApiController]
public class TaskController : ControllerBase
{
    private readonly ZohoTimesheetDBContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CommonService _commonService;
    private readonly IConfiguration _configuration;


    public TaskController(ZohoTimesheetDBContext context, IHttpClientFactory httpClientFactory, CommonService commonService, IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _commonService = commonService;
        _configuration = configuration;
    }

    //Fetch all tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDetail>>> GetTasks()
    {
        return await _context.TaskDetails.Where(t => !t.IsDeleted).ToListAsync();
    }

    // Fetch a specific task by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDetail>> GetTask(int id)
    {
        var task = await _context.TaskDetails.FindAsync(id);
        if (task == null || task.IsDeleted)
        {
            return NotFound();
        }
        return task;
    }

    //Create a new task
    [HttpPost]
    public async Task<ActionResult<TaskDetail>> CreateTask(TaskModel taskmodel, string userZuid)
    {
        var task = new TaskDetail
        {
            ProjectId = taskmodel.ProjectId,
            ProjectTaskId = taskmodel.ProjectTaskId,
            FromDate = taskmodel.FromDate,
            ToDate = taskmodel.ToDate,
            Notes = taskmodel.Notes,
            CreatedBy = userZuid,
            IsDeleted = false
        };

        _context.TaskDetails.Add(task);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTask), new { id = task.ID }, task);
    }

    //Fetch existing tasks + Zoho tasks
    [HttpGet("fetch-with-zoho")]
    public async Task<List<TaskDetailModel>> GetTasksWithZoho(string userId, DateTime start_date)
    {
        try
        {
            Log.Information("Starting GetTaskswithZoho API...");

            var timesheets = await _context.Timesheets.Where(t => (t.Status == (int)TimesheetStatus.Approved || t.Status == (int)TimesheetStatus.Rejected) && t.FromDate.Date == start_date && t.UserZuid == userId).ToListAsync();
            if (timesheets.Count == 0)
            {
                var tasks = await _context.TaskDetails.Where(t => !t.IsDeleted && t.FromDate.Date == start_date && t.CreatedBy == userId).ToListAsync();
                var zohoTasks = await GetUserTimesheetLogs(userId, start_date);

                Log.Information("tasks count" + tasks.Count());


                List<Tasklog> zohoData = new List<Tasklog>();
                if (!string.IsNullOrEmpty(zohoTasks.ToString()))
                {
                    var deserializedZohoData = JsonConvert.DeserializeObject<ZohoTimelogModel>(zohoTasks.ToString());
                    if (deserializedZohoData?.Timelogs?.Date != null)
                    {
                        zohoData = deserializedZohoData.Timelogs.Date.SelectMany(d => d.Tasklogs).ToList();
                    }
                }
                var mergedList = tasks.Select(d => new TaskDetailModel
                {
                    ID = d.ID.ToString(),
                    FromDate = d.FromDate,
                    ToDate = d.ToDate,
                    Notes = d.Notes,
                    IsDeleted = d.IsDeleted,
                    Project = _context.Projects.Where(c => c.ID == Convert.ToInt32(d.ProjectId)).Select(c => c.Name).FirstOrDefault(),
                    Task = _context.ProjectTasks.Where(c => c.ID == Convert.ToInt32(d.ProjectTaskId)).Select(c => c.Name).FirstOrDefault(),
                    SubTask = null,
                    TotalMinutes = null,
                    BillStatus = null,
                    Approver = null
                }).ToList();

                if (zohoData.Any())
                {

                    mergedList.AddRange(zohoData.Select(l => new TaskDetailModel
                    {
                        ID = null,
                        FromDate = string.IsNullOrEmpty(l.start_time) ? Convert.ToDateTime(l.created_date) : Convert.ToDateTime(l.created_date).Date.AddHours(DateTime.ParseExact(l.start_time.Trim(), "HH:mm", CultureInfo.InvariantCulture).Hour).AddMinutes(DateTime.ParseExact(l.start_time.Trim(), "HH:mm", CultureInfo.InvariantCulture).Minute),
                        ToDate = string.IsNullOrEmpty(l.end_time) ? Convert.ToDateTime(l.created_date) : Convert.ToDateTime(l.created_date).Date.AddHours(DateTime.ParseExact(l.end_time.Trim(), "HH:mm", CultureInfo.InvariantCulture).Hour).AddMinutes(DateTime.ParseExact(l.end_time.Trim(), "HH:mm", CultureInfo.InvariantCulture).Minute),
                        Notes = l.notes,
                        IsDeleted = null,
                        Project = l.project?.Name,
                        Task = l.task_list?.Name,
                        SubTask = l.task?.Name,
                        TotalMinutes = l.total_minutes,
                        BillStatus = l.bill_status,
                        Approver = l.allowed_approver?.Name
                    }));

                }

                return mergedList;
            }
            else
            {
                if (timesheets.Any(t => t.Status == (int)TimesheetStatus.Rejected))
                {
                    var query = from task in _context.TaskDetails
                                join timesheet in _context.Timesheets
                                on task.ID equals timesheet.InternalTaskId into timesheetGroup
                                from timesheet in timesheetGroup.DefaultIfEmpty() // Left Join
                                where task.CreatedBy == userId
                                 && task.FromDate.Date == start_date.Date
                                && timesheet.InternalTaskId == null // Checking for NULL values in Timesheets
                                select task;

                    var result = query.ToList();
                    var zohoTasks = await GetUserTimesheetLogs(userId, start_date);
                    List<Tasklog> zohoData = new List<Tasklog>();
                    if (!string.IsNullOrEmpty(zohoTasks.ToString()))
                    {
                        var deserializedZohoData = JsonConvert.DeserializeObject<ZohoTimelogModel>(zohoTasks.ToString());
                        if (deserializedZohoData?.Timelogs?.Date != null)
                        {
                            zohoData = deserializedZohoData.Timelogs.Date.SelectMany(d => d.Tasklogs).ToList();
                        }
                    }
                    var mergedlist = result.Select(d => new TaskDetailModel
                    {
                        ID = d.ID.ToString(),
                        FromDate = d.FromDate,
                        ToDate = d.ToDate,
                        Notes = d.Notes,
                        IsDeleted = d.IsDeleted,
                        Project = _context.Projects.Where(c => c.ID == Convert.ToInt32(d.ProjectId)).Select(c => c.Name).FirstOrDefault(),
                        Task = _context.ProjectTasks.Where(c => c.ID == Convert.ToInt32(d.ProjectTaskId)).Select(c => c.Name).FirstOrDefault(),
                        SubTask = null,
                        TotalMinutes = null,
                        BillStatus = null,
                        Approver = null
                    }).ToList();
                    if (zohoData.Any())
                    {

                        mergedlist.AddRange(zohoData.Select(l => new TaskDetailModel
                        {
                            ID = null,
                            FromDate = string.IsNullOrEmpty(l.start_time) ? Convert.ToDateTime(l.created_date) : Convert.ToDateTime(l.created_date).Date.AddHours(DateTime.ParseExact(l.start_time.Trim(), "HH:mm", CultureInfo.InvariantCulture).Hour).AddMinutes(DateTime.ParseExact(l.start_time.Trim(), "HH:mm", CultureInfo.InvariantCulture).Minute),
                            ToDate = string.IsNullOrEmpty(l.end_time) ? Convert.ToDateTime(l.created_date) : Convert.ToDateTime(l.created_date).Date.AddHours(DateTime.ParseExact(l.end_time.Trim(), "HH:mm", CultureInfo.InvariantCulture).Hour).AddMinutes(DateTime.ParseExact(l.end_time.Trim(), "HH:mm", CultureInfo.InvariantCulture).Minute),
                            Notes = l.notes,
                            IsDeleted = null,
                            Project = l.project?.Name,
                            Task = l.task_list?.Name,
                            SubTask = l.task?.Name,
                            TotalMinutes = l.total_minutes,
                            BillStatus = l.bill_status,
                            Approver = l.allowed_approver?.Name
                        }));

                    }
                    return mergedlist;
                }
                return timesheets.Select(d => new TaskDetailModel
                {
                    ID = d.ID.ToString(),
                    FromDate = d.FromDate,
                    ToDate = d.ToDate,
                    Notes = d.Notes,
                    IsDeleted = null,
                    Project = d.Project,
                    Task = d.TaskName,
                    SubTask = d.SubTaskName,
                    TotalMinutes = d.TotalMinutes,
                    BillStatus = d.BillStatus,
                    Approver = _context.Approvers.Where(c => c.AppZuid == d.Approverzuid).Select(c => c.AppName).FirstOrDefault(),
                    TaskStatus = d.Status
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return null;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitTimesheet([FromBody] TimesheetRequest request)
    {
        try
        {
            if (request.TimesheetList == null || !request.TimesheetList.Any())
            {
                return BadRequest(new { message = "Timesheet list cannot be empty." });
            }

            foreach (var item in request.TimesheetList)
            {
                item.CreatedAt = DateTime.UtcNow;


                var timesheetEntity = new Timesheet
                {
                    UserZuid = item.UserZuid,
                    Approverzuid = item.Approverzuid,
                    ApproverEmail = item.ApproverEmail,
                    FromDate = (DateTime)item.FromDate,
                    ToDate = (DateTime)item.ToDate,
                    TotalMinutes = item.TotalMinutes,
                    TaskName = item.TaskName,
                    SubTaskName = item.SubTaskName,
                    Notes = item.Notes,
                    Status = (int)TimesheetStatus.Pending,
                    Project = item.Project,
                    BillStatus = item.BillStatus,
                    CreatedAt = DateTime.UtcNow,
                    SourceId = item.SourceId,
                    InternalTaskId = item.InternalTaskId,
                    isActive = true
                };

                _context.Timesheets.Add(timesheetEntity);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Timesheet submitted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest();
        }
    }

    [HttpGet("timesheet-isexists")]
    public async Task<ActionResult<IEnumerable<Timesheet>>> GetIsTimesheetExists(DateTime searchDate, string userid)
    {
        return await _context.Timesheets.Where(t => t.FromDate.Date == searchDate.Date && t.UserZuid == userid && t.Status != (int)TimesheetStatus.Rejected).ToListAsync();
    }

    [HttpGet("approver")]
    public async Task<ActionResult<IEnumerable<object>>> GetPendingTimesheet(string ApproverId, string? groupBy = null, string? filterbyUser = null, DateTime? filterbyDate = null)
    {
        var query = _context.Timesheets
            .Where(t => t.Approverzuid == ApproverId && t.Status == (int)TimesheetStatus.Pending)
               .Join(_context.Approvers,
              t => t.UserZuid,
              a => a.EmpZuid,
              (t, a) => new
              {
                  t.ID,
                  t.UserZuid,
                  t.Approverzuid,
                  t.ApproverEmail,
                  t.FromDate,
                  t.ToDate,
                  t.TotalMinutes,
                  t.TaskName,
                  t.SubTaskName,
                  t.Notes,
                  t.Status,
                  t.Project,
                  t.BillStatus,
                  t.CreatedAt,
                  t.UpdatedAt,
                  t.SourceId,
                  t.InternalTaskId,
                  t.isActive,
                  t.RejectionReason,
                  a.EmpName // Fetching Employee Name from Approver table
              });

        if (filterbyUser != null)
        {
            query = query.Where(e => e.UserZuid == filterbyUser);
        }

        if (filterbyDate.HasValue)
        {
            query = query.Where(g => g.FromDate.Date == filterbyDate.Value.Date);
        }

        // If no groupBy parameter is provided, return raw data
        var result = await query.OrderBy(q => q.FromDate).ToListAsync();
        return Ok(result);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.TaskDetails.FindAsync(id);
        if (task == null || task.IsDeleted)
        {
            return NotFound();
        }

        task.IsDeleted = true;  // Soft delete
        await _context.SaveChangesAsync();

        return NoContent();
    }

    #region PrivateFunctions
    private async Task<object> GetUserTimesheetLogs(string userId, DateTime start_date)
    {
        if (!Request.Headers.TryGetValue("Authorization", out StringValues accessToken))
        {
            return Unauthorized("Access token is missing.");
        }
    
        var client = _httpClientFactory.CreateClient();

        // Step 1️: Fetch Portal ID
        var portalRequest = new HttpRequestMessage(HttpMethod.Get, _configuration["Zoho:ApiBaseUrl"] + "portals/");
        portalRequest.Headers.Add("Authorization", accessToken.ToString());

        var portalResponse = await client.SendAsync(portalRequest);
        if (!portalResponse.IsSuccessStatusCode)
        {
            try
            {
                // Check for unauthorized (401) status and refresh token
                if (portalResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var newToken = await _commonService.RefreshToken(_commonService.GetUserDetailsbyID(userId).Result.refresh_token, userId);  // Refresh token logic
                    if (newToken == null)
                    {
                        return Unauthorized("Failed to refresh access token.");
                    }

                    // Retry the portal request with the new token
                    // Create a new HttpRequestMessage with the same method, URI, and content
                    var newRequest = new HttpRequestMessage(portalRequest.Method, portalRequest.RequestUri)
                    {
                        Content = portalRequest.Content
                    };

                    // Set headers again
                    newRequest.Headers.Add("Authorization", "Bearer " + newToken.ToString());

                    // Retry the portal request with the new request
                    portalResponse = await client.SendAsync(newRequest);
                    accessToken = "Bearer " + newToken;
                }

                if (!portalResponse.IsSuccessStatusCode)
                {
                    return StatusCode((int)portalResponse.StatusCode, "Failed to fetch Zoho portals.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        var portalContent = await portalResponse.Content.ReadAsStringAsync();
        var portalData = JsonSerializer.Deserialize<ZohoPortalsResponse>(portalContent);

        if (portalData?.portals == null || portalData.portals.Count == 0)
        {
            return BadRequest("No portals found.");
        }

        string portalId = portalData.portals[0].id.ToString(); // Use the first portal ID

        // Step 2️: Fetch Timesheet Logs Using Portal ID

        DateTime parsedStartDate = DateTime.Parse(start_date.ToString());  // Convert to DateTime
        string formatted_start_date = parsedStartDate.ToString("MM-dd-yyyy");

        string logsUrl = $"{_configuration["Zoho:ApiBaseUrl"]}portal/{portalId}/logs?users_list={userId}&view_type=day&date={formatted_start_date}&bill_status=All&component_type=task";

        var logsRequest = new HttpRequestMessage(HttpMethod.Get, logsUrl);
        logsRequest.Headers.Add("Authorization", accessToken.ToString());

        var logsResponse = await client.SendAsync(logsRequest);
        return await logsResponse.Content.ReadAsStringAsync();
    }
    #endregion
}
