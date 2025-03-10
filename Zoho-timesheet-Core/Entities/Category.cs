using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zoho_timesheet_Core.Entities
{
    public class Category
    {
        private int _ID;
        private string _CategoryName;


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int ID { get { return this._ID; } set { this._ID = value; } }
        public string CategoryName { get { return this._CategoryName; } set { this._CategoryName = value; } }
    }
}
