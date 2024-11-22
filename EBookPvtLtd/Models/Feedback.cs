using System.ComponentModel.DataAnnotations;

namespace EBookPvtLtd.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }
        [Required]
        public int BookId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Feedback cannot exceed 1000 characters.")]
        public string Content { get; set; }

        [Required]
        public string CustomerName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Book? Book { get; set; }
    }
}
