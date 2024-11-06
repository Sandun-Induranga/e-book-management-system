using EBookPvtLtd.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace EBookPvtLtd.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        public string UserId { get; set; } // Link to Identity User
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime JoinedDate { get; set; } = DateTime.Now;

        // Navigation property
        public Users User { get; set; }
    }
}
