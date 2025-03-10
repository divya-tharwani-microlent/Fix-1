using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;
using Zoho_timesheet_API.Enums;
using Zoho_timesheet_API.Models;
using Zoho_timesheet_API.Services;
using Zoho_timesheet_Core.Entities;
using Zoho_timesheet_EFC;
using static System.Runtime.InteropServices.JavaScript.JSType;

[Route("api/[controller]")]
[ApiController]
public class ApproverController : ControllerBase
{
    private readonly ZohoTimesheetDBContext _context;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    ISesSmtpEmailService _emailService;

    public ApproverController(ZohoTimesheetDBContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory, ISesSmtpEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _emailService = emailService;
    }


    [HttpGet("fetch-zohoproject-users")]
    public async Task<List<object>> FetchZohoProjectUsers(string authToken)
    {
        try
        {
            // 1. Get Portal ID
            var portalResponse = await GetApiResponse($"{_configuration["Zoho:ApiBaseUrl"]}portals/", authToken);
            var portalId = portalResponse?.RootElement.GetProperty("portals")[0].GetProperty("id").GetUInt64().ToString();
            if (string.IsNullOrEmpty(portalId))
                throw new Exception("Portal ID not found!");

            // 2. Get All Projects
            var projectsRequest = new HttpRequestMessage(HttpMethod.Get, $"{_configuration["Zoho:ApiBaseUrl"]}portal/{portalId}/projects/");
            projectsRequest.Headers.Add("Authorization", $"Bearer {authToken}");

            var client = _httpClientFactory.CreateClient();
            var projectsResponse = await client.SendAsync(projectsRequest);
            var projectsJson = await projectsResponse.Content.ReadAsStringAsync();
            var projectsObj = JObject.Parse(projectsJson);
            var projectsArray = JsonSerializer.Deserialize<List<ZohoProject>>(projectsObj["projects"].ToString());

            var userSet = new HashSet<object>();

            foreach (var project in projectsArray)
            {
                // 3. Get Users for Each Project
                var usersRequest = new HttpRequestMessage(HttpMethod.Get, project.link.user.url);
                usersRequest.Headers.Add("Authorization", $"Bearer {authToken}");

                var usersResponse = await client.SendAsync(usersRequest);
                var usersJson = await usersResponse.Content.ReadAsStringAsync();
                var usersObj = JObject.Parse(usersJson);
                var usersArray = JsonSerializer.Deserialize<List<User>>(usersObj["users"].ToString());

                foreach (var user in usersArray)
                {
                    userSet.Add(new
                    {
                        UserId = user.id,
                        Name = user.name,
                        Role = user.role,
                        IsActive = user.active,
                        Email = user.email
                    });
                }
            }

            return userSet.ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching Zoho project users: {ex.Message}");
        }
    }

    private async Task<JsonDocument> GetApiResponse(string url, string authToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", $"Bearer {authToken}");
        var client = _httpClientFactory.CreateClient();

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }

    [HttpPost("save-approver")]
    public async Task<IActionResult> SaveApprover([FromBody] Approver approver)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);


        // Check if both Employee Email and Approver Email already exist
        var existingApproverPair = await _context.Approvers
            .FirstOrDefaultAsync(a => a.EmpEmail == approver.EmpEmail && a.ApproverEmail == approver.ApproverEmail && a.IsActive && !a.IsDeleted);

        if (existingApproverPair != null)
        {
            return Conflict(new { message = "This employee already has this approver assigned." });
        }

        // Check if the employee already has an active approver
        var existingActiveApprover = await _context.Approvers
            .FirstOrDefaultAsync(a => a.EmpEmail == approver.EmpEmail && a.IsActive && !a.IsDeleted);

        if (existingActiveApprover != null)
        {
            return Conflict(new { message = "Approver already exists for this employee. Do you want to update?", id = existingActiveApprover.ID });
        }

        _context.Approvers.Add(approver);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Approver saved successfully!" });
    }

    [HttpPut("update-approver")]
    public async Task<IActionResult> UpdateApprover([FromBody] Approver approver, int ID)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Find the existing active approver for the employee
        var existingApprover = await _context.Approvers
            .FirstOrDefaultAsync(a => a.ApproverEmail == approver.ApproverEmail && a.EmpEmail == approver.EmpEmail && a.IsActive && !a.IsDeleted);

        if (existingApprover != null)
        {
            return Conflict(new { message = "Approver already exists." });
        }

        var approverEntity = await _context.Approvers.FindAsync(ID);
        if (approverEntity != null)
        {
            // Update the existing approver with new details
            approverEntity.EmpName = approver.EmpName;
            approverEntity.EmpEmail = approver.EmpEmail;
            approverEntity.EmpZuid = approver.EmpZuid;
            approverEntity.ApproverEmail = approver.ApproverEmail;
            approverEntity.AppName = approver.AppName;
            approverEntity.AppZuid = approver.AppZuid;
            approverEntity.IsActive = approver.IsActive;

            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "Approver updated successfully!" });
    }


    [HttpGet("approver-list")]
    public async Task<IActionResult> GetApprovers()
    {
        var approvers = await _context.Approvers.Where(a => a.IsDeleted == false).ToListAsync();
        return Ok(approvers);
    }

    [HttpPost("delete-approver/{id}")]
    public async Task<IActionResult> DeleteApprover(int id)
    {
        var approver = await _context.Approvers.FindAsync(id);
        if (approver == null)
        {
            return NotFound(new { message = "Approver not found." });
        }

        approver.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Approver deleted successfully!" });
    }

    [HttpPost("deactive-approver/{id}")]
    public async Task<IActionResult> DeactiveApprover(int id, [FromQuery] bool isActive)
    {
        var approver = await _context.Approvers.FindAsync(id);
        if (approver == null)
        {
            return NotFound(new { message = "Approver not found." });
        }

        approver.IsActive = isActive; // Update IsActive flag
        await _context.SaveChangesAsync();

        string statusMessage = isActive ? "Approver reactivated successfully!" : "Approver deactivated successfully!";
        return Ok(new { message = statusMessage, id = approver.ID });
    }

    [HttpGet("get-approver")]
    public async Task<IActionResult> GetApproverbyEmpId(string empId)
    {
        var approver = await _context.Approvers.FirstOrDefaultAsync(a => a.IsDeleted == false && a.IsActive && a.EmpZuid == empId);
        return Ok(approver);
    }

    [HttpGet("approver/{approverId}")]
    public async Task<IActionResult> GetTimesheetsForApproval(string approverId)
    {
        var timesheets = await _context.Timesheets
            .Where(t => t.Approverzuid == approverId && t.Status == (int)TimesheetStatus.Pending)
            .ToListAsync();

        return Ok(timesheets);
    }

    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateTimesheetStatus([FromBody] List<TimesheetModel> timesheetUpdates)
    {
        if (timesheetUpdates == null || !timesheetUpdates.Any())
            return BadRequest("Invalid request. No timesheets provided.");

        var timesheetIds = timesheetUpdates.Select(t => t.ID).ToList();
        var timesheets = await _context.Timesheets.Where(t => timesheetIds.Contains(t.ID)).ToListAsync();

        if (!timesheets.Any())
            return NotFound("No matching timesheets found.");


        foreach (var timesheet in timesheets)
        {
            var updateData = timesheetUpdates.FirstOrDefault(t => t.ID == timesheet.ID);
            if (updateData != null)
            {
                timesheet.Status = updateData.Status;
                if (updateData.Status == (int)TimesheetStatus.Rejected)
                {
                    timesheet.isActive = false;
                    timesheet.RejectionReason = updateData.RejectionReason;
                }

                timesheet.UpdatedAt = DateTime.UtcNow;
            }
        }
        if (timesheetUpdates.Any(t => t.Status == (int)TimesheetStatus.Rejected))
        {
            var EmployeeName = await _context.Approvers.Where(a => a.EmpZuid == timesheets[0].UserZuid).Select(a => a.EmpName).FirstOrDefaultAsync();
            // Create the HTML table
            var tableHtml = CreateHtmlTable(timesheetUpdates);

            string mailBody = "<div class=\"content\">\r\n    <p>Dear " + EmployeeName + ",</p>\r\n    <p>We regret to inform you that your timesheet for the period <strong>" + timesheets[0].FromDate.ToString("MMM d, yyyy") + " - " + timesheets[0].ToDate.ToString("MMM d, yyyy") + "</strong> has been <strong>rejected</strong> by the approver.</p>\r\n    <p><strong>Rejection Reason:</strong>" + timesheetUpdates[0].RejectionReason + "</p>\r\n    <p>Please review the details below and make the necessary corrections.</p>\r\n    \r\n    " + tableHtml + "    \r\n    <p>If you have any questions, please contact your manager or approver.</p>\r\n</div>";

            var usermail = await _context.Approvers.Where(a => a.EmpZuid == timesheets[0].UserZuid).Select(a => a.EmpEmail).FirstOrDefaultAsync();
            if (usermail != null)
            {
                var mailstatus = "";
                if (await _emailService.SendEmailAsync("richam@quantratech.com", usermail, "Timesheet Rejected Notification", mailBody))
                {
                    mailstatus = "Email sent successfully.";
                }
                else
                {
                    mailstatus = "Failed to send email.";
                }
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = $"Status updated for timesheets of {timesheets[0].FromDate:MMM d, y}." });
    }

    private string CreateHtmlTable(List<TimesheetModel> data)
    {
        if (data == null || data.Count == 0)
        {
            return "<p>No data to display.</p>";
        }

        var sb = new StringBuilder();
        sb.Append("<table border='1'>");

        // Create table header
        sb.Append("<tr>");

        sb.Append($"<th>Project</th>");
        sb.Append($"<th>Task</th>");
        sb.Append($"<th>Sub Task</th>");
        sb.Append($"<th>From Date</th>");
        sb.Append($"<th>To Date</th>");
        sb.Append($"<th>Total Minutes</th>");
        sb.Append($"<th>Bill Status</th>");
        sb.Append($"<th>Notes</th>");
        sb.Append($"<th>Status</th>");

        sb.Append("</tr>");

        // Create table rows
        foreach (var item in data)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{item.Project}</td>");
            sb.Append($"<td>{item.TaskName}</td>");
            sb.Append($"<td>{item.SubTaskName}</td>");
            sb.Append($"<td>{item.FromDate}</td>");
            sb.Append($"<td>{item.ToDate}</td>");
            sb.Append($"<td>{item.TotalMinutes}</td>");
            sb.Append($"<td>{item.BillStatus}</td>");
            sb.Append($"<td>{item.Notes}</td>");
            TimesheetStatus statusEnum = (TimesheetStatus)Enum.ToObject(typeof(TimesheetStatus), item.Status);
            sb.Append($"<td>{statusEnum.ToString()}</td>");
            sb.Append("</tr>");
        }

        sb.Append("</table>");
        return sb.ToString();
    }
}
