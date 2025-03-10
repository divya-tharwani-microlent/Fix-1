namespace Zoho_timesheet_API.Models
{
    public class TaskModel
    {
        public int ID { get; set; }
        public int ProjectId { get; set; }
        public int ProjectTaskId {  get; set; }
        public DateTime FromDate {  get; set; }
        public DateTime ToDate {  get; set; }
        public string Notes {  get; set; }
    }
}
