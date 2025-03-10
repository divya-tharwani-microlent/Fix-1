using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zoho_timesheet_Core.Entities
{
    public class Timesheet
    {
        private int _ID;
        private string _UserZuid;
        private string _Approverzuid;
        private string _ApproverEmail;
        private DateTime _FromDate;
        private DateTime _ToDate;
        private int _TotalMinutes;
        private string? _TaskName;
        private string? _SubTaskName;
        private string _Notes;
        private int _Status;
        private string? _Project;
        private string? _BillStatus;
        private DateTime _CreatedAt;
        private DateTime _UpdatedAt;
        private int _SourceId;
        private int? _InternalTaskId;
        private bool _IsActive;
        private string? _RejectionReason;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int ID { get { return this._ID; } set { this._ID = value; } }
        public string UserZuid { get { return this._UserZuid; } set { this._UserZuid = value; } }
        public string Approverzuid { get { return this._Approverzuid; } set { this._Approverzuid = value; } }
        public string ApproverEmail { get { return this._ApproverEmail; } set { this._ApproverEmail = value; } }
        public DateTime FromDate { get { return this._FromDate; } set { this._FromDate = value; } }
        public DateTime ToDate { get { return this._ToDate; } set { this._ToDate = value; } }
        public int TotalMinutes { get { return this._TotalMinutes; } set { this._TotalMinutes = value; } }
        public string? TaskName { get { return this._TaskName; } set { this._TaskName = value; } }
        public string? SubTaskName { get { return this._SubTaskName; } set { this._SubTaskName = value; } }
        public string Notes { get { return this._Notes; } set { this._Notes = value; } }
        public int Status { get { return this._Status; } set { this._Status = value; } }
        public string? Project { get { return this._Project; } set { this._Project = value; } }
        public string? BillStatus { get { return this._BillStatus; } set { this._BillStatus = value; } }
        public DateTime CreatedAt { get { return this._CreatedAt; } set { this._CreatedAt = value; } }
        public DateTime UpdatedAt { get { return this._UpdatedAt; } set { this._UpdatedAt = value; } }
        public int SourceId { get { return this._SourceId; } set { this._SourceId = value; } }
        public Source source { get; set; }
        public int? InternalTaskId { get { return this._InternalTaskId; } set { this._InternalTaskId = value; } }
        public TaskDetail task { get; set; }
        public bool isActive {get { return this._IsActive; } set { this._IsActive = value; } }
        public string? RejectionReason { get { return this._RejectionReason; } set { this._RejectionReason = value; } }
    }
}
