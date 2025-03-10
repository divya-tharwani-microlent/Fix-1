namespace Zoho_timesheet_API.Models
{
    public class ZohoProject
    {
        public long id { get; set; }
        public string name { get; set; }
        public Link link { get; set; }
    }

    public class Link
    {
        public User user { get; set; }
    }

    public class User
    {
        public string url { get; set; } // for ZohoProject

        public string name { get; set; }
        public string email { get; set; }
        public string id { get; set; }
        public string role { get;set; }
        public bool active { get;set; }
    }
}
