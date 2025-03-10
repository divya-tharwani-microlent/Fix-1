namespace Zoho_timesheet_API.Models
{
    public class TimesheetModel
    {
        public int ID { get;set; }
        public string UserZuid{ get; set; }
        public string Approverzuid { get; set; }
        public string? ApproverEmail { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int TotalMinutes { get; set; }
        public string? TaskName { get; set; }
        public string? SubTaskName { get; set; }
        public string Notes { get; set; }
        public int Status { get; set; }
        public string? Project { get; set; }
        public string? BillStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int SourceId { get; set; }
        public int? InternalTaskId { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class TimesheetRequest
    {
        public List<TimesheetModel> TimesheetList { get; set; }
    }
}
