using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FastPMS.Models.Domain;

namespace FastPMS.Models
{
    public class LiveChat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        // Navigation Properties
        [ForeignKey("SenderId")]
        public virtual Users Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual Users Receiver { get; set; }
    }
}