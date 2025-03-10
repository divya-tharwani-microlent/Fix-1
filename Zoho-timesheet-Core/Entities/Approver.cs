using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zoho_timesheet_Core.Entities
{
    public class Approver
    {
        private int _ID;
        private string _EmpName;
        private string _EmpEmail;
        private string _EmpZuid;
        private string _ApproverEmail;
        private string _AppName;
        private string _AppZuid;
        private bool _IsActive;
        private bool _IsDeleted;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get { return this._ID; } set { this._ID = value; } }
        public string EmpName { get { return this._EmpName; } set { this._EmpName = value; } }
        public string EmpEmail { get { return this._EmpEmail; } set { this._EmpEmail = value; } }
        public string EmpZuid { get { return this._EmpZuid; } set { this._EmpZuid = value; } }
        public string ApproverEmail { get { return this._ApproverEmail; } set { this._ApproverEmail = value; } }
        public string AppName { get { return this._AppName; } set { this._AppName = value; } }
        public string AppZuid { get { return this._AppZuid; } set { this._AppZuid = value; } }
        public bool IsActive { get { return this._IsActive; } set { this._IsActive = value; } }
        public bool IsDeleted { get { return this._IsDeleted; } set { this._IsDeleted = value; } }
    }

}
