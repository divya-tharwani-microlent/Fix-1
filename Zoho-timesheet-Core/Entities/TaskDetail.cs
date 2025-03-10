using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zoho_timesheet_Core.Entities
{
    public class TaskDetail
    {
        private int _ID;
        private int _ProjectId;
        private int _ProjectTaskId;
        private DateTime _FromDate;
        private DateTime _ToDate;
        private string _Notes = string.Empty;
        private string _CreatedBy = string.Empty;
        private bool _IsDeleted;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int ID { get { return this._ID; } set { this._ID = value; } }
        public int ProjectId { get { return this._ProjectId; } set { this._ProjectId = value; } }
        public Project project { get; set; }
        public int ProjectTaskId { get { return this._ProjectTaskId; } set { this._ProjectTaskId = value; } }
        public ProjectTask projecttask { get; set; }
        public DateTime FromDate { get { return this._FromDate; } set { this._FromDate = value; } }
        public DateTime ToDate { get { return this._ToDate; } set { this._ToDate = value; } }
        public string Notes { get { return this._Notes; } set { this._Notes = value; } }
        public string CreatedBy { get { return this._CreatedBy; } set { this._CreatedBy = value; } }
        public bool IsDeleted { get { return this._IsDeleted; } set { this._IsDeleted = value; } }
        public ICollection<Timesheet> TaskForTimesheet { get; set; }

    }
}
