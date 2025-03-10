using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zoho_timesheet_Core.Entities
{
    public class Source
    {
        private int _ID;
        private string _Name;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get { return this._ID; } set { this._ID = value; } }
        public string Name { get { return this._Name;} set { this._Name = value; } }

        public ICollection<Timesheet> SourceForTimesheet { get; set; }

    }
}
