using System;

namespace DatabaseProject.Models
{
    public class EmployeeTicketViewModel
    {
        public int TicketID { get; set; }
        public string ProductName { get; set; }
        public string CustomerName { get; set; }
        public DateTime DateOpened { get; set; }
        public string Status { get; set; }
        public string IssueDescription { get; set; }
    }
}