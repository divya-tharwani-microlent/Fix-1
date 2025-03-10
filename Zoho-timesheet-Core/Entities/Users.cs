using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zoho_timesheet_Core.Entities
{
    public class Users
    {
        private int _ID;
        private string _First_Name;
        private string _Last_Name;
        private string _Display_Name;
        private string _Email;
        private string _ZUID;
        private string? _access_token;
        private string? _refresh_token;
        private DateTime _Createddate;


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int ID { get { return this._ID; } set { this._ID = value; } }
        public string First_Name { get { return this._First_Name; } set { this._First_Name = value; } }
        public string Last_Name { get { return this._Last_Name; } set { this._Last_Name = value; } }
        public string Display_Name { get { return this._Display_Name; } set { this._Display_Name = value; } }
        public string Email { get { return this._Email; } set { this._Email = value; } }
        public string ZUID { get { return this._ZUID; } set { this._ZUID = value; } }
        public string? access_token { get { return this._access_token; } set { this._access_token = value; } }
        public string? refresh_token { get { return this._refresh_token; } set { this._refresh_token = value; } }
        public DateTime Createddate { get { return this._Createddate; } set { this._Createddate = value; } }
        public bool isValid()
        {
            return this.First_Name != null && this.Email != null && this.ZUID != null;
        }
    }
}
