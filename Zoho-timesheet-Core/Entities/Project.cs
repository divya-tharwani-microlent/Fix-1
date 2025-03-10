using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zoho_timesheet_Core.Entities
{
    public class Project
    {
        private int _ID;
        private string _Name;
        private bool _IsActive;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int ID { get { return this._ID; } set { this._ID = value; } }
        public string Name { get { return this._Name; } set { this._Name = value; } }
        public bool IsActive { get { return this._IsActive; } set { this._IsActive = value; } }
        public ICollection<ProjectTask> TaskForProject { get; set; }
        public ICollection<TaskDetail> InternalTaskforProject { get; set; }
    }
}
