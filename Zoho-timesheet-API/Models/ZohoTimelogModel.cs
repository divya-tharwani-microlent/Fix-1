using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Zoho_timesheet_API.Models
{
    public class ZohoTimelogModel
    {
        public Timelogs Timelogs { get; set; }
    }

    public class Timelogs
    {
        public List<DateEntry> Date { get; set; }
        public string Role { get; set; }
        public string NonBillableHours { get; set; }
        public string BillableHours { get; set; }
        public string GrandTotal { get; set; }
    }

    public class DateEntry
    {
        public string Date { get; set; }
        public string DisplayFormat { get; set; }
        public long DateLong { get; set; }
        public string TotalHours { get; set; }
        public List<Tasklog> Tasklogs { get; set; }
    }

    public class Tasklog
    {
        public string notes { get; set; }
        public AllowedApprover allowed_approver { get; set; }
        public string owner_id { get; set; }
        public string last_modified_date { get; set; }
        public ProjectModel project { get; set; }
        public object last_modified_time_long { get; set; }
        public AddedBy added_by { get; set; }
        public object id { get; set; }
        public TaskList task_list { get; set; }
        public object created_time_long { get; set; }
        public int hours { get; set; }
        public string owner_name { get; set; }
        public string created_time_format { get; set; }
        public int minutes { get; set; }
        public int total_minutes { get; set; }
        public string approval_status { get; set; }
        public List<object> custom_fields { get; set; }
        public string end_time { get; set; }
        public string bill_status { get; set; }
        public string start_time { get; set; }
        public string last_modified_time_format { get; set; }
        public Task task { get; set; }
        public string id_string { get; set; }
        public string created_date { get; set; }
        public string hours_display { get; set; }
    }

    public class AllowedApprover
    {
        public string Zpuid { get; set; }
        public string Name { get; set; }
        public string Zuid { get; set; }
    }

    public class ProjectModel
    {
        public string Name { get; set; }
        public string IdString { get; set; }
        public string Id { get; set; }
    }

    public class AddedBy
    {
        public string Zpuid { get; set; }
        public string Name { get; set; }
        public string Zuid { get; set; }
    }

    public class Task
    {
        public string SubTaskLevel { get; set; }
        public string ParentTaskId { get; set; }
        public bool IsSubTask { get; set; }
        public string RootTaskId { get; set; }
        public string Name { get; set; }
        public string IdString { get; set; }
        public string Id { get; set; }
        public bool IsParent { get; set; }
    }

    public class TaskList
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }

}
