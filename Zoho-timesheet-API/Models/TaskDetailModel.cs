namespace Zoho_timesheet_API.Models
{
    public class TaskDetailModel 
    {
        public string? ID { get; set; }
        public DateTime? FromDate {  get; set; }
        public DateTime? ToDate {  get; set; }
        public string Notes { get; set; }=string.Empty;
        public bool? IsDeleted {  get; set; }
        public string? Project { get; set; }
        public string? Task { get; set; }
        public string? SubTask {  get; set; }
        public int? TotalMinutes {  get; set; }
        public string? BillStatus {  get; set; }
        public string? Approver {  get; set; }
        public int? TaskStatus { get; set; }
    }
}
